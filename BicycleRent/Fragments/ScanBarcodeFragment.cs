using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using ZXing.Mobile;
using static Android.Manifest;
using BicycleRent.Interfaces;

namespace BicycleRent
{
    public class ScanBarcodeFragment : Fragment
    {
        ZXingSurfaceView _scanner;
        MobileBarcodeScanningOptions _scanOptions;
        Action<Fragment, string> _scanCallback;
        FrameLayout _flCamera;

        public static ScanBarcodeFragment NewInstance(Action<Fragment, string> action) =>
        new ScanBarcodeFragment(action) { Arguments = new Bundle() };

        public ScanBarcodeFragment(Action<Fragment, string> scanCallback)
        {
            _scanCallback = scanCallback;
            _scanOptions = new MobileBarcodeScanningOptions();
            _scanOptions.DelayBetweenContinuousScans = 3000;
        }

        private void StartCamera()
        {
            try
            {
                _scanner = new ZXingSurfaceView(this.Activity, _scanOptions);

                _flCamera.AddView(_scanner, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create Surface View Failed: " + ex);
            }

            _scanner.StartScanning((obj) =>
            {
                _scanner.StopScanning();
                _scanCallback?.Invoke(this, obj.Text);
            }
        , _scanOptions);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.ScanBarcodeFragment, container, false);

            _flCamera = view.FindViewById<FrameLayout>(Resource.Id.FrameLayout);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            StartCamera();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _scanner?.StopScanning();
        }
    }
}