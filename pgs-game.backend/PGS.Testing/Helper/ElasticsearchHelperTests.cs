using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Utility.Extensions;
using target.Utility.Helper.Elasticsearch.Model;

namespace target.Testing
{
    [TestClass]
    public class ElasticsearchHelperTests : BaseTest
    {
        [TestMethod]
        public void GetEachApiErrorCount_Tests()
        {
            string json = @"{
                ""took"": 5,
                ""timed_out"": false,
                ""_shards"": {
                    ""total"": 1,
                    ""successful"": 1,
                    ""skipped"": 0,
                    ""failed"": 0
                },
                ""hits"": {
                    ""total"": {
                        ""value"": 1466,
                        ""relation"": ""eq""
                    },
                    ""max_score"": null,
                    ""hits"": []
                },
                ""aggregations"": {
                    ""group_by_category"": {
                        ""doc_count_error_upper_bound"": 0,
                        ""sum_other_doc_count"": 128,
                        ""buckets"": [
                            {
                                ""key"": ""/v1/game-records"",
                                ""doc_count"": 1120
                            },
                            {
                                ""key"": ""/v1/wallet/amount"",
                                ""doc_count"": 145
                            }
                        ]
                    }
                }
            }";

            var result = json.JsonConvertToModel<ESGetEachApiErrorCountResponse>();

            Assert.AreEqual(5, result.took);
            Assert.IsFalse(result.timed_out);
            Assert.AreEqual(1466L, result.hits.total.value);
            Assert.AreEqual("eq", result.hits.total.relation);
            Assert.IsNull(result.hits.max_score);
            Assert.AreEqual(0, result.hits.hits.Count);
            Assert.AreEqual(128L, result.aggregations.group_by_category.sum_other_doc_count);
            Assert.AreEqual(2, result.aggregations.group_by_category.buckets.Count);
            Assert.AreEqual("/v1/game-records", result.aggregations.group_by_category.buckets[0].key);
            Assert.AreEqual(1120L, result.aggregations.group_by_category.buckets[0].doc_count);
        }

        [TestMethod]
        public void GetEachHourCount_in48Hours_Tests()
        {
            string json = @"{
                ""took"": 47,
                ""timed_out"": false,
                ""_shards"": {
                    ""total"": 1,
                    ""successful"": 1,
                    ""skipped"": 0,
                    ""failed"": 0
                },
                ""hits"": {
                    ""total"": {
                        ""value"": 3932,
                        ""relation"": ""eq""
                    },
                    ""max_score"": null,
                    ""hits"": []
                },
                ""aggregations"": {
                    ""hourly_counts"": {
                        ""buckets"": [
                            {
                                ""key_as_string"": ""2026-09-02T07:00:00.000Z"",
                                ""key"": 1788332400000,
                                ""doc_count"": 65
                            },
                            {
                                ""key_as_string"": ""2026-09-02T08:00:00.000Z"",
                                ""key"": 1788336000000,
                                ""doc_count"": 58
                            }
                        ]
                    }
                }
            }";

            var result = json.JsonConvertToModel<ESGetEachHourCountIn48HoursResponse>();

            Assert.AreEqual(47, result.took);
            Assert.IsFalse(result.timed_out);
            Assert.AreEqual(3932L, result.hits.total.value);
            Assert.AreEqual("eq", result.hits.total.relation);
            Assert.IsNull(result.hits.max_score);
            Assert.AreEqual(0, result.hits.hits.Count);
            Assert.AreEqual(2, result.aggregations.hourly_counts.buckets.Count);
            Assert.AreEqual("2026-09-02T07:00:00.000Z", result.aggregations.hourly_counts.buckets[0].key_as_string);
            Assert.AreEqual(1788332400000L, result.aggregations.hourly_counts.buckets[0].key);
            Assert.AreEqual(65L, result.aggregations.hourly_counts.buckets[0].doc_count);
        }
    }
}
