#region license

// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Linq;
using MbUnit.Framework;
using SolrNet.Mapping;
using SolrNet.Mapping.Validation;
using SolrNet.Mapping.Validation.Rules;
using SolrNet.Schema;
using SolrNet.Tests.Utils;

namespace SolrNet.Tests {
    [TestFixture]
    public class ValidationRuleTests {

        [Test]
        public void RequiredSolrFieldForWhichNoCopyFieldExistsShouldReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));


            var schemaManager = new MappingValidator(mgr, new[] {new RequiredFieldsAreMappedRule()});

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(1, validationResults.Count);
        }

        [Test]
        public void MappedPropertyForWhichSolrFieldExistsInSchemaShouldNotReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");

            var schemaManager = new MappingValidator(mgr, new[] { new MappedPropertiesIsInSolrSchemaRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(0, validationResults.Count);
        }

        [Test]
        public void MappedPropertyForWhichDynamicFieldExistsInSchemaShouldNotReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Producer"), "producer_s");

            var schemaManager = new MappingValidator(mgr, new[] { new MappedPropertiesIsInSolrSchemaRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(0, validationResults.Count);
        }

        [Test]
        public void MappedPropertyForWhichNoSolrFieldOrDynamicFieldExistsShouldReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("ID"), "id");
            mgr.SetUniqueKey(typeof (SchemaMappingTestDocument).GetProperty("ID"));
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("Name"), "name");
            mgr.Add(typeof (SchemaMappingTestDocument).GetProperty("FieldNotSolrSchema"), "FieldNotSolrSchema");

            var schemaManager = new MappingValidator(mgr, new[] { new MappedPropertiesIsInSolrSchemaRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaBasic.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(1, validationResults.Count);
        }

        [Test]
        public void MappedPropertiesIsInSolrSchemaRule_ignores_score() {
            var rule = new MappedPropertiesIsInSolrSchemaRule();
            var mapper = new MappingManager();
            mapper.Add(typeof(SchemaMappingTestDocument).GetProperty("Score"), "score");
            var results = rule.Validate(typeof (SchemaMappingTestDocument), new SolrSchema(), mapper).ToList();
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void StringMappedToIntShouldReturnError() {
            var mappingTypesCompatibleRule = new MappingTypesAreCompatibleWithSolrTypesRule(new[] {new StringSolrFieldTypeChecker()});

            var mgr = new MappingManager();
            mgr.Add(typeof(SchemaMappingTestDocument).GetProperty("FieldNotSolrSchema"), "popularity");

            var schemaManager = new MappingValidator(mgr, new[] { mappingTypesCompatibleRule });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaMappingTypes.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(1, validationResults.Count);           
        }

        [Test]
        public void StringMappedToStringShouldNotReturnError()
        {
            var mappingTypesCompatibleRule = new MappingTypesAreCompatibleWithSolrTypesRule(new[] {new StringSolrFieldTypeChecker()});

            var mgr = new MappingManager();
            var schemaManager = new MappingValidator(mgr, new[] { mappingTypesCompatibleRule });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaMappingTypes.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(0, validationResults.Count);
        }

        [Test]
        public void MappingTypesAreCompatibleWithSolrTypesRule_with_nonexistant_rule() {
            var rule = new MappingTypesAreCompatibleWithSolrTypesRule(new[]{new StringSolrFieldTypeChecker()});
            var mappingManager = new MappingManager();
            mappingManager.Add(typeof(SchemaMappingTestDocument).GetProperty("Name"));
            var validations = rule.Validate(typeof(SchemaMappingTestDocument), new SolrSchema(), mappingManager).ToList();

        }

        [Test]
        public void MutivaluedSolrFieldNotMappedToCollectionShouldReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof(SchemaMappingTestDocument).GetProperty("Name"), "name");

            var schemaManager = new MappingValidator(mgr, new[] { new MultivaluedMappedToCollectionRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaMultiValuedName.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(1, validationResults.Count);
        }

        [Test]
        public void MultivaluedSolrFieldMappedToCollectionShouldNotReturnError() {
            var mgr = new MappingManager();
            mgr.Add(typeof(SchemaMappingTestDocument).GetProperty("NameCollection"), "name");

            var schemaManager = new MappingValidator(mgr, new[] { new MultivaluedMappedToCollectionRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaMultiValuedName.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(0, validationResults.Count);
        }

        [Test]
        public void CollectionMappedToNonMultiValuedFolrFieldShouldReturnError()
        {
            var mgr = new MappingManager();
            mgr.Add(typeof(SchemaMappingTestDocument).GetProperty("NameCollection"), "author");

            var schemaManager = new MappingValidator(mgr, new[] { new MultivaluedMappedToCollectionRule() });

            var schemaXmlDocument = EmbeddedResource.GetEmbeddedXml(GetType(), "Resources.solrSchemaMultiValuedName.xml");
            var solrSchemaParser = new SolrSchemaParser();
            var schema = solrSchemaParser.Parse(schemaXmlDocument);

            var validationResults = schemaManager.Validate(typeof(SchemaMappingTestDocument), schema).ToList();
            Assert.AreEqual(1, validationResults.Count);
        }
    }
}