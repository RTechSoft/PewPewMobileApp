﻿using pewpewmobileapp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pewpewmobileapp
{
    class ApprovalRequest
    {
        const double PIx = 3.141592653589793;
        const double RADIUS = 6371;

        double minDistance = -1;

        Asset assetNearest;

        private async Task<Asset> getNearestAsset(Town borrower, string assetName)
        {
            assetNearest = null;
            var assets = await App.MobileService.GetTable<Asset>().ToListAsync();
            var organizations = await App.MobileService.GetTable<Organization>().ToListAsync();

            double latitude = borrower.latitude;
            double longitude = borrower.longitude;

            foreach (Asset asset in assets)
            {
                foreach (Organization organization in organizations)
                {
                    if (asset.name.Equals(assetName) && asset.owner.Equals(organization.name) && asset.status.Equals("AVAILABLE"))
                    {
                        double distance = computeDistance(longitude, latitude, organization.longitude, organization.latitude);

                        if (minDistance == -1)
                        {
                            minDistance = distance;
                            assetNearest = asset;
                        }

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            assetNearest = asset;
                        }

                    }
                }
            }

            return assetNearest;

        }

        public double Radians(double x)
        {
            return x * PIx / 180;
        }

        public double computeDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return angle * RADIUS;
        }

        public async Task<List<Asset>> GetListOfAsset()
        {
            List<Asset> listAsset = await App.MobileService.GetTable<Asset>().ToListAsync();
            return listAsset;
        }


        public async void RequestAsset(int quantity, string assetName, Town requester)
        {
            List<Asset> assets = await GetListOfAsset();
            for (int i = 0; i < quantity; i++)
            {
                Asset foundAsset = await getNearestAsset(requester, assetName);
                if (foundAsset != null)
                {
                    foundAsset.borrower = requester.contactperson;
                    foundAsset.status = "ON THE WAY";
                    await App.MobileService.GetTable<Asset>().UpdateAsync(foundAsset);
                }
                else
                {
                    Pending pendingticket = new Pending();
                    pendingticket.assetName = assetName;
                    pendingticket.townID = requester.id;
                    await App.MobileService.GetTable<Pending>().InsertAsync(pendingticket);
                }
            }

        }

        public async void AddAsset(Asset asset)
        {
            await App.MobileService.GetTable<Asset>().InsertAsync(asset);
        }
        
    }
}
