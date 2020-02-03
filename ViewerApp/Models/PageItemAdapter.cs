using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Java.Lang;
using ViewerApp.AL;
using Bumptech.Glide;
using System.IO;

namespace ViewerApp.Models
{
    class PageItemAdapter : PagerAdapter
    {
        Context context;
        public List<IPageItem> pageItems;
        string libraryPath;

        public PageItemAdapter(Context context, string libraryPath) {
            this.context = context;
            this.libraryPath = libraryPath;
        }

        public override int Count {
            get { return pageItems.Count; }
        }

        public override Java.Lang.Object InstantiateItem(View container, int position) {
            var imageView = new ImageView(context);
            string fullCoverPath = Path.Combine(libraryPath,pageItems[position].GetPath());
            Glide.With(context).Load(fullCoverPath).FitCenter().Into(imageView);

            var viewPager = container.JavaCast<ViewPager>();
            viewPager.AddView(imageView);
            return imageView;
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object view) {
            var viewPager = container.JavaCast<ViewPager>();
            viewPager.RemoveView(view as View);
        }


        public override bool IsViewFromObject(View view, Java.Lang.Object obj) {
            return view == obj;
        }

        // Display a caption for each Tree page in the PagerTitleStrip:
        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) {
            string fileName = Path.GetFileName(pageItems[position].GetPath());
            return new Java.Lang.String(fileName);
        }
    }
}