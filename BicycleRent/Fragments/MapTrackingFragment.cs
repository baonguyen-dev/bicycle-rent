using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Support.V4.App;
using Android.Locations;
using BicycleRent.Interfaces;

namespace BicycleRent.Fragments
{
    public class MapTrackingFragment : Fragment, IOnMapReadyCallback
    {
        private Android.Gms.Maps.GoogleMapOptions _googleMapOptions;
        private EventHandler<string> OnLatLngChanged;
        private MarkerOptions marker = new MarkerOptions();
        public static GoogleMap _googleMap;
        public static MapTrackingFragment NewInstance() => new MapTrackingFragment();
        public static MapTrackingFragment NewInstance(Android.Gms.Maps.GoogleMapOptions googleMapOptions) => new MapTrackingFragment(googleMapOptions);

        public MapTrackingFragment()
        {

        }

        public void OnMapReady(GoogleMap map)
        {

            _googleMap = map;
        }

        public void MarkerChangePosition(LatLng latLng)
        {
            if (_googleMap != null)
            {
                _googleMap.Clear();
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(latLng);
                builder.Zoom(12);
                builder.Bearing(155);
                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
                marker.SetPosition(latLng);
                marker.SetTitle("HERE");

                _googleMap.AddMarker(marker);
                _googleMap.MoveCamera(cameraUpdate);
                _googleMap.MapType = GoogleMap.MapTypeNormal;
            }
        }

        public MapTrackingFragment(Android.Gms.Maps.GoogleMapOptions googleMapOptions)
        {
            _googleMapOptions = googleMapOptions;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.MapTrackingFragment, container, false);
            var mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);
            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            this.Dispose();
        }
    }
}