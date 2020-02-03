using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SharedLibrary;
using SharedLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViewerApp.AL;
using ViewerApp.DAL;
using ViewerApp.Models;
using Xamarin.Essentials;
using SimpleInjector;
using Android.Support.V4.View;

namespace ViewerApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : Activity
    {
        #region Recyclerview
        ViewGroup containerMain;
        RecyclerView recyclerView1;
        RecyclerView recyclerView2;
        CardItemAdapter adapter1;
        CardItemAdapter adapter2;
        #endregion

        #region ViewPager
        ViewPager viewPager;
        PageItemAdapter pageAdapter;
        #endregion

        IAppLogic al;

        SceneManager sm;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            containerMain = FindViewById<ViewGroup>(Resource.Id.container_main);

            sm = new SceneManager();
            VerifyStoragePermissions(this);

            var fa = new FileSystemAccess();
            al = new AppLogic(new ExternalStorageLogic(fa, new AlbumInfo()), new InternalStorageLogic(fa), new AlbumInfo());
            HandleAppResponse(al.Initialize());
        }

        private void VerifyStoragePermissions(Activity activity) {
            string[] permissionStorages = {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
            };
            var permission = ActivityCompat.CheckSelfPermission(activity, Manifest.Permission.WriteExternalStorage);

            if(permission != Permission.Granted) {
                ActivityCompat.RequestPermissions(activity, permissionStorages, 1);
            }
        }

        protected override void OnStart() {
            base.OnStart();

            NavigateScene(sm.Push("home:primary"));
        }

        protected override void OnResume() {
            base.OnResume();
        }

        public override void OnBackPressed() {
            NavigateScene(sm.Pop());
        }

        public override bool OnContextItemSelected(IMenuItem item) {
            if(item.ItemId == 10) { //DELETE
                string action = adapter1.CardItems[item.GroupId].GetAction();
                Toast.MakeText(this, action, ToastLength.Short).Show();
                HandleAppResponse(al.DeleteAlbum(action));

                adapter1.CardItems.RemoveAt(item.GroupId);
                adapter1.NotifyDataSetChanged();
            }
            if(item.ItemId == 11) { //EDIT

            }
            if(item.ItemId == 12) { //QUICK SCAN
                HandleAppResponse(al.QuickScan());
            }
            if(item.ItemId == 13) { //FULL SCAN
                HandleAppResponse(al.FullScan());
            }
            if(item.ItemId == 14) { //SAVE
                HandleAppResponse(al.SaveDb());
            }
            if(item.ItemId == 15) { //DEBUG GETDB
                HandleAppResponse(al.DEBUGGetDb());
            }
            if(item.ItemId == 16) { //TEST UNDELETE
                al.UndoDeletes();
            }
            return base.OnContextItemSelected(item);
        }


        void OnItemClick1(object sender, int position) {
            string action = adapter1.CardItems[position].GetAction();
            if(action.Split(':')[0] == "query") {
                NavigateScene(sm.Push(action));
            }
            else if(action.Split(':')[0] == "album") {
                NavigateScene(sm.Push(action));
            }
        }

        void OnItemClick2(object sender, int position) {
            string action = adapter2.CardItems[position].GetAction();
            NavigateScene(sm.Push(action));
        }

        void NavigateScene(string scene) {
            if(string.IsNullOrEmpty(scene)) return;
            string sceneType = scene.ActionType();
            string sceneQuery = scene.ActionQuery();

            //if(false) { 
            if(sceneType == "home" || sceneType == "query") {
                //containerMain.RemoveAllViews();
                if(containerMain.GetChildAt(0) == null) {
                    LayoutInflater.Inflate(Resource.Layout.PageQuery, containerMain, true);
                }
                if(containerMain.GetChildAt(1) != null) {
                    containerMain.GetChildAt(1).Visibility = ViewStates.Gone;
                }
                containerMain.GetChildAt(0).Visibility = ViewStates.Visible;

                #region RecyclerView
                if(recyclerView1 == null) {
                    recyclerView1 = FindViewById<RecyclerView>(Resource.Id.recyclerView1);
                    recyclerView1.SetLayoutManager(new GridLayoutManager(this, 5, GridLayoutManager.Vertical, false));
                }
                if(recyclerView2 == null) {
                    recyclerView2 = FindViewById<RecyclerView>(Resource.Id.recyclerView2);
                    recyclerView2.SetLayoutManager(new GridLayoutManager(this, 5, GridLayoutManager.Vertical, false));
                }
                #endregion

                #region Adapter
                if(adapter1 == null) {
                    adapter1 = new CardItemAdapter(al.LEAKGetLibraryPath());
                    adapter1.ItemClick += OnItemClick1;
                    recyclerView1.SetAdapter(adapter1);
                }
                if(adapter2 == null) {
                adapter2 = new CardItemAdapter(al.LEAKGetLibraryPath());
                    adapter2.ItemClick += OnItemClick2;
                    recyclerView2.SetAdapter(adapter2);
                }
                #endregion

                var cardItems1 = new List<ICardItem>();
                var cardItems2 = new List<ICardItem>();

                if(sceneType == "home") {
                    cardItems1 = HandleAppResponse(al.GetGenreCards());
                    cardItems2 = HandleAppResponse(al.GetArtistCards(0));
                }
                else if(sceneType == "query") {
                    cardItems1 = HandleAppResponse(al.GetAlbumCards(sceneQuery));
                }

                adapter1.CardItems = cardItems1;
                adapter1.NotifyDataSetChanged();
                adapter2.CardItems = cardItems2;
                adapter2.NotifyDataSetChanged();
            }
            else if(sceneType == "album") {
                //sceneQuery = "Artists/Akatsuki Souken/[Akatsuki Souken] A former girl professional wrestler mom and bullying kid [English]";
                if(containerMain.GetChildAt(1) == null) {
                    LayoutInflater.Inflate(Resource.Layout.TreePager, containerMain);
                }
                if(containerMain.GetChildAt(0) != null) {
                    containerMain.GetChildAt(0).Visibility = ViewStates.Gone;
                }
                containerMain.GetChildAt(1).Visibility = ViewStates.Visible;

                if(viewPager == null) {
                    viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
                }

                pageAdapter = new PageItemAdapter(this, al.LEAKGetLibraryPath());
                pageAdapter.pageItems = HandleAppResponse(al.GetPageItems(sceneQuery));
                viewPager.Adapter = pageAdapter;

                #region OLD Solution. Problem: old images still lingering
                //if(pageAdapter == null) {
                //    pageAdapter = new PageItemAdapter(this, al.LEAKGetLibraryPath());
                //}
                //pageAdapter.pageItems = HandleAppResponse(al.GetPageItems(sceneQuery));
                //pageAdapter.NotifyDataSetChanged();
                //viewPager.SetCurrentItem(0, false);
                //if(viewPager.Adapter == null) {
                //    viewPager.Adapter = pageAdapter;
                //}
                #endregion
            }
        }

        //Every call to AppRespone must go through this method
        dynamic HandleAppResponse(AppResponse response) {
            if(!response.Status) {
                Toast.MakeText(this, response.Message, ToastLength.Short).Show();
            }
            Logger.Log(response);

            return response.Data;
        }
    }
}