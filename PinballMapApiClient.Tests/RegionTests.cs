using System;
using System.Threading.Tasks;
using Xunit;

namespace PinballMapApiClient.Tests
{
    public class RegionTests
    {
        [Fact]
        public async Task Region_GetAllRegions_ShouldHaveValue()
        {
            var apiClient = new PinballMapApiClient();
            apiClient.Log = x => System.Diagnostics.Debug.WriteLine($"{x}");

            var regions = await apiClient.GetAllRegionsAsync();

            Assert.NotNull(regions);
            Assert.True(regions.IsSuccessfulWithResult);
            Assert.NotEmpty(regions.Result.Regions);
        }
    }
}
