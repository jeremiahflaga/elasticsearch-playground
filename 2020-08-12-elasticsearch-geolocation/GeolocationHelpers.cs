using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearchGeolocationExample
{
    static class GeolocationHelpers
    {
        // Generate random locations nearby: https://gis.stackexchange.com/a/68275
        public static GeoLocation GenerateRandomGeoLocation(GeoLocation baseGeolocation, double radius)
        {
            double x0 = baseGeolocation.Longitude;
            double y0 = baseGeolocation.Latitude;
            Random random = new Random();

            // Convert radius from meters to degrees
            double radiusInDegrees = radius / 111000f;

            double u = random.NextDouble();
            double v = random.NextDouble();
            double w = radiusInDegrees * Math.Sqrt(u);
            double t = 2 * Math.PI * v;
            double x = w * Math.Cos(t);
            double y = w * Math.Sin(t);

            // Adjust the x-coordinate for the shrinking of the east-west distances
            double new_x = x / Math.Cos(y0 * (Math.PI / 180));

            double foundLongitude = new_x + x0;
            double foundLatitude = y + y0;
            return GeoLocation.TryCreate(foundLatitude, foundLongitude);
        }
    }
}
