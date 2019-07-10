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
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Runtime.InteropServices;
using ZXing.Mobile;
using static Android.Views.View;
namespace BicycleRent
{
    [Activity(Label = "DashBoard", Theme = "@style/AppTheme")]
    public class DashBoardActivity : AppCompatActivity, IOnClickListener, IOnCompleteListener, IValueEventListener
    {
        int REQUEST_CAMERA = 0;
        private TextView txtWelcome;
        private EditText input_new_password;
        private Button btnChangePass, btnLogout, _btnScan;
        private RelativeLayout activity_dashboard;
        private FirebaseAuth auth;
        private string _FirebaseURL = "https://bicycle-rent.firebaseio.com/";
        private DatabaseReference myQR;
        private FirebaseDatabase _database;
        private string _value;

        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.dashboard_btn_change_pass)
                ChangePassword(input_new_password.Text);
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
            myQR = _database.GetReference("QR");
            myQR.AddValueEventListener(this);
            //View  
            btnChangePass = FindViewById<Button>(Resource.Id.dashboard_btn_change_pass);
            txtWelcome = FindViewById<TextView>(Resource.Id.dashboard_welcome);
            btnLogout = FindViewById<Button>(Resource.Id.dashboard_btn_logout);
            input_new_password = FindViewById<EditText>(Resource.Id.dashboard_newpassword);
            activity_dashboard = FindViewById<RelativeLayout>(Resource.Id.activity_main);
            _btnScan = FindViewById<Button>(Resource.Id.btnScan);
            _btnScan.Click += _btnScan_Click;
            btnChangePass.SetOnClickListener(this);
            btnLogout.SetOnClickListener(this);
            //Check Session  
            if (auth != null)
                txtWelcome.Text = "Welcome , " + auth.CurrentUser.Email;
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
                                    SetLockValue(scanResult);
                                    ((ScanBarcodeFragment)fragment).FragmentManager.BeginTransaction().Remove(fragment).Commit();
                                }
                            }
                        });
                        ReplaceFragment(scanBarcodeFragment);
                    });
                }
            }
        }

        private void ReplaceFragment(Android.Support.V4.App.Fragment fragment)
        {
            var ff = SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fContent, fragment);
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

        private void SetLockValue(string myValue)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                if (_value.Contains(','))
                {
                    var _lockValue = _value.Split(',')[1];
                    if (_lockValue == "0")
                    {
                        myQR.SetValueAsync(myValue + ',' + "1");
                    }
                    else myQR.SetValueAsync(myValue + ',' + "0");
                }
                else
                {
                    if(_value == "0")
                    {
                        myQR.SetValueAsync(myValue + ',' + "1");
                    }
                    else myQR.SetValueAsync(myValue + ',' + "0");
                }
            }
            else
            {
                myQR.SetValueAsync(myValue + ',' + "1");
            }
        }

        public void OnCancelled(DatabaseError error)
        {
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Exists())
            {
                _value = snapshot.GetValue(true).ToString();
            }
        }
    }
}