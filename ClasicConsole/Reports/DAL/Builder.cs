using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Utils;

namespace ClasicConsole.Reports.DAL
{
    public abstract class BaseModelBuilder<TModel> : IModelBuilder<TModel>
    {

        protected PropertyInfo[] ModelProperties { get; private set; }

        public BaseModelBuilder()
        {
            ModelProperties = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.CanRead)
                .ToArray();
        }

        public virtual TModel BuildDefaultModel()
        {
            return typeof(TModel).IsValueType ? default(TModel) : Activator.CreateInstance<TModel>();
        }

        /// <summary>
        /// Build a single model instance from the DB reader using a set of model properties to bind to.
        /// </summary>
        /// <param name="dbDataReader"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected abstract TModel BuildSingleModel(IDataReader dbDataReader, PropertyInfo[] properties);



        public TModel Build(IDataReader dbDataReader)
        {
            // build the model using the PropertyInfo[] list
            TModel constructedModel = BuildSingleModel(dbDataReader, ModelProperties);

            if ((constructedModel != null) && (ModelReady != null))
            {
                // raise event
                // handle the newly constructed model
                ModelReady(constructedModel);
            }

            return constructedModel;
        }

        public TModel BuildScalar(IDataReader dbDataReader)
        {
            // read the first column from the DB reader
            return (TModel)dbDataReader.ReadColumn(0);
        }

        /// <summary>
        /// Event that is fired after the model is materialized
        /// </summary>
        public event ModelMaterializedEventHandler<TModel> ModelReady;
    }
    public class DefaultModelBuilder<TModel> : BaseModelBuilder<TModel> where TModel : new()
    {


        protected override TModel BuildSingleModel(IDataReader dbDataReader, PropertyInfo[] modelProperties)
        {
            TModel model = new TModel();

            foreach (PropertyInfo modelProperty in modelProperties)
            {
                object modelPropertyValue = null;
                try
                {
                    if (dbDataReader.ColumnExists(modelProperty.Name))
                    {
                        // if the model property was found in the returned
                        // SQL response
                        modelPropertyValue = dbDataReader.ReadColumn(modelProperty.Name);
                        modelProperty.SetValue(model, modelPropertyValue);
                    }
                }
                catch (ArgumentException argEx)
                {
                    // cannot set the model property
                    string errorMsg = @"Builder failed to set value on the model property [{0}] of model type [{1}]. 
                                        Value to set: {2}
                                        Type of value to set: {3} ";
                    throw new Exception(string.Format(errorMsg,
                            modelProperty.Name,
                            typeof(TModel).Name,
                            modelPropertyValue ?? "NULL",
                            (modelPropertyValue != null) ? (object)modelPropertyValue.GetType() : "<none>"),
                        argEx);
                }
            }

            return model;
        }
    }


    public sealed class ScalarValueBuilder<TScalar> : BaseModelBuilder<TScalar>
    {

        protected override TScalar BuildSingleModel(IDataReader dbDataReader, System.Reflection.PropertyInfo[] properties)
        {
            object scalarValue = dbDataReader.GetValue(0);

            if (typeof(TScalar).Equals(typeof(StringAdapter)))
            {
                // in case we're building the StringAdapter model
                scalarValue = new StringAdapter(
                    ((scalarValue != null) && !DBNull.Value.Equals(scalarValue)) ?
                        scalarValue.ToString() :
                        (string)null
                );
            }

            return (TScalar)scalarValue;
        }
    }

    public sealed class StringAdapter
    {
        private string _stringValue;

        public StringAdapter(string value)
        {
            _stringValue = value;
        }

        public StringAdapter()
        {
            // empty NEW constructor
        }

        /// <summary>
        /// Value of the internal <seealso cref="string"/> object
        /// </summary>
        public string Value
        {
            get { return _stringValue; }
            set { _stringValue = value; }
        }

        public override string ToString()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _stringValue;
        }
    }
}
