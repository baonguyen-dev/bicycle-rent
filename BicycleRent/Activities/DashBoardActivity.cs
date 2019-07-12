using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BicycleRent.Interfaces;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ZXing.Mobile;
using Java.Lang;
using static Android.Views.View;
using System.Linq;
using BicycleRent.Fragments;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;

namespace BicycleRent
{
    [Activity(Label = "DashBoard", Theme = "@style/AppTheme")]
    public class DashBoardActivity : AppCompatActivity, IOnClickListener, IOnCompleteListener, IValueEventListener
    {
        int REQUEST_CAMERA = 0;
        private TextView _txtWelcome;
        private EditText _input_new_password;
        private Button _btnChangePass, _btnLogout, _btnScan, _btnShowMap;
        private RelativeLayout _activity_dashboard;
        private FirebaseAuth auth;
        private string _FirebaseURL = "https://bicycle-rent.firebaseio.com/";
        private DatabaseReference _dbRef;
        private FirebaseDatabase _database;
        private string _value, _qrValue1, _qrValue2;
        private MapTrackingFragment _map;
        private Android.Support.V4.App.FragmentManager _fragmentManager;

        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.dashboard_btn_change_pass)
                ChangePassword(_input_new_password.Text);
            else if (v.Id == Resource.Id.dashboard_btn_logout)
                LogoutUser();
        }
        private void LogoutUser()
        {
            auth.SignOut();
            if (auth.CurrentUser == null)
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
        }
        private void ChangePassword(string newPassword)
        {
            FirebaseUser user = auth.CurrentUser;
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.UpdatePassword(newPassword)
                .AddOnCompleteListener(this);
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DashBoard);
            //Init Firebase  
            auth = FirebaseAuth.GetInstance(MainActivity.app);
            //Init Database
            _database = FirebaseDatabase.GetInstance(_FirebaseURL);
            InitChild();
            //View  
            _btnChangePass = FindViewById<Button>(Resource.Id.dashboard_btn_change_pass);
            _txtWelcome = FindViewById<TextView>(Resource.Id.txtWelcome);
            _btnLogout = FindViewById<Button>(Resource.Id.dashboard_btn_logout);
            _input_new_password = FindViewById<EditText>(Resource.Id.dashboard_newpassword);
            _activity_dashboard = FindViewById<RelativeLayout>(Resource.Id.activity_main);
            _btnScan = FindViewById<Button>(Resource.Id.btnScan);
            _btnShowMap = FindViewById<Button>(Resource.Id.btnShowMap);
            _btnScan.Click += _btnScan_Click;
            _btnShowMap.Click += _btnShowMap_Click;
            _btnChangePass.SetOnClickListener(this);
            _btnLogout.SetOnClickListener(this);
            //Check Session  
            if (auth != null)
                _txtWelcome.Text = "Welcome , " + auth.CurrentUser.Email;
        }

        private void _btnShowMap_Click(object sender, EventArgs e)
        {
            _map = MapTrackingFragment.NewInstance();
            ReplaceFragment(_map);
        }

        private void _btnScan_Click(object sender, System.EventArgs e)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) != Permission.Granted)
                {
                    if (!ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.Camera))
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.Camera }, REQUEST_CAMERA);
                    }
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        var scanBarcodeFragment = new ScanBarcodeFragment((fragment, scanResult) =>
                        {
                            if (!string.IsNullOrEmpty(scanResult))
                            {
                                if (fragment.IsVisible && fragment is ScanBarcodeFragment)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        ((ScanBarcodeFragment)fragment).FragmentManager.BeginTransaction().Remove(fragment).Commit();
                                        SetLockValue(scanResult);
                                    });
                                }
                            }
                        });
                        ReplaceFragment(scanBarcodeFragment);
                    });
                }
            }
        }

        private void InitChild()
        {
            _dbRef = _database.Reference;
            _dbRef.AddValueEventListener(this);
        }

        private void ReplaceFragment(Android.Support.V4.App.Fragment fragment)
        {
            _fragmentManager = ((FragmentActivity)this).SupportFragmentManager;
            var ff = _fragmentManager.BeginTransaction()
                .AddToBackStack(fragment.ToString())
                .Replace(Resource.Id.fContent, fragment);
            if (!SupportFragmentManager.IsStateSaved)
            {
                ff.Commit();
            }
            else
            {
                ff.CommitAllowingStateLoss();
            }
        }

        public void OnComplete(Task task)
        {
            if (task.IsSuccessful == true)
            {
                Toast.MakeText(this, "Password has been Changed!", ToastLength.Long).Show();
            }
        }

        private void SetLockValue(string qrValue)
        {
            if (!string.IsNullOrEmpty(qrValue))
            {
                var text1 = _qrValue1.Split(',')[0];
                var num1 = _qrValue1.Split(',')[1];
                var text2 = _qrValue2.Split(',')[0];
                var num2 = _qrValue2.Split(',')[1];

                if (qrValue == text1)
                {
                    if (num1 == "1")
                    {
                        _dbRef.Child("QR1").SetValueAsync(qrValue + "," + "0");
                    }
                    else
                    {
                        _dbRef.Child("QR1").SetValueAsync(qrValue + "," + "1");
                    }
                }
                else if (qrValue == text2)
                {
                    if (num2 == "1")
                    {
                        _dbRef.Child("QR2").SetValueAsync(qrValue + "," + "0");
                    }
                    else
                    {
                        _dbRef.Child("QR2").SetValueAsync(qrValue + "," + "1");
                    }
                }
                else
                {
                    Toast.MakeText(this, "Bike Not Found", ToastLength.Long).Show();
                }
            }
        }

        public override void OnBackPressed()
        {
            if (_fragmentManager.BackStackEntryCount > 0)
            {
                _fragmentManager.PopBackStackImmediate();
            }
            else
                base.OnBackPressed();
        }

        public void OnCancelled(DatabaseError error)
        {
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Exists())
            {
                foreach (var snapChild in snapshot.Children?.ToEnumerable<DataSnapshot>())
                {
                    if (snapChild.Key.ToString() == "LatLng")
                    {
                        _value = snapChild.GetValue(true).ToString();
                        LatLng latLng = new LatLng(double.Parse(_value.Split(',')[0]), double.Parse(_value.Split(',')[1]));
                        if (_map != null)
                            _map.MarkerChangePosition(latLng);
                    }
                    if (snapChild.Key.ToString() == "QR1")
                    {
                        _qrValue1 = snapChild.GetValue(true).ToString();
                    }
                    if (snapChild.Key.ToString() == "QR2")
                    {
                        _qrValue2 = snapChild.GetValue(true).ToString();
                    }
                    if (snapChild.Key.ToString() == "Cycle")
                    {
                        Toast.MakeText(this, snapChild.GetValue(true).ToString() + " " + "km", ToastLength.Long).Show();
                    }
                }
            }
        }
    }
}