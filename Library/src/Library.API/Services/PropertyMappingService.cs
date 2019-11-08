using Library.API.Entities;
using Library.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            var authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                { "Genre", new PropertyMappingValue(new List<string>() { "Genre" } )},
                { "Age", new PropertyMappingValue(new List<string>() { "DateOfBirth" } , true) },
                { "Name", new PropertyMappingValue(new List<string>() { "FirstName", "LastName" }) }
            };
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue>  GetPropertyMapping
            <TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            var propertyMappings = matchingMapping.ToList();
            if (propertyMappings.Count() == 1)
            {
                return propertyMappings.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            return (from field in fieldsAfterSplit 
                select field.Trim() into trimmedField 
                let indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.Ordinal) 
                select indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace))
                .All(propertyName => propertyMapping.ContainsKey(propertyName));
        }

    }
}
