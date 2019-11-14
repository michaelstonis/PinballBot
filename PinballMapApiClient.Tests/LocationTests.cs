using System;
using System.Threading.Tasks;
using Xunit;

namespace PinballMapApiClient.Tests
{
    public class LocationTests
    {
        [Fact]
        public async Task Location_GetAllLocations_ShouldHaveValue()
        {
            var apiClient = new PinballMapApiClient();
            apiClient.Log = x => System.Diagnostics.Debug.WriteLine($"{x}");
            apiClient.IncludeFullResponse = true;

            var locations = await apiClient.GetLocationsClosestByAddress("chicago", true, 5);

            Assert.NotNull(locations);
            Assert.True(locations.IsSuccessfulWithResult);
            Assert.NotEmpty(locations.Result.Locations);
        }
    }
}
