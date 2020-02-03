using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using ViewerApp.AL;
using Bumptech.Glide;
using SharedLibrary;

namespace ViewerApp.Models
{
    public class CardViewHolder : RecyclerView.ViewHolder, View.IOnCreateContextMenuListener
    {
        Context context;
        List<string> languages;

        ICardItem cardItem;

        public ImageView Cover { get; private set; }
        public TextView Caption { get; private set; }
        public ProgressBar ProgressBar { get; private set; }
        public LinearLayout ContainerFlag { get; set; }
        public List<ImageView> LanguageFlags { get; private set; }
        public ImageView New { get; private set; }

        
        public CardViewHolder(Context context, View itemView, Action<int> listener) : base(itemView) {
            languages = new List<string> { "English", "Japanese", "Chinese", "Other" };
            this.context = context;

            Cover = itemView.FindViewById<ImageView>(Resource.Id.cvImageCover);
            Caption = itemView.FindViewById<TextView>(Resource.Id.cvTextView);
            ProgressBar = itemView.FindViewById<ProgressBar>(Resource.Id.cvProgressBar);
            ContainerFlag = itemView.FindViewById<LinearLayout>(Resource.Id.cvContainerFlag);
            LanguageFlags = new List<ImageView>();
            LanguageFlags.Add(itemView.FindViewById<ImageView>(Resource.Id.cvImageFlag0));
            LanguageFlags.Add(itemView.FindViewById<ImageView>(Resource.Id.cvImageFlag1));
            LanguageFlags.Add(itemView.FindViewById<ImageView>(Resource.Id.cvImageFlag2));
            LanguageFlags.Add(itemView.FindViewById<ImageView>(Resource.Id.cvImageFlag3));
            New = itemView.FindViewById<ImageView>(Resource.Id.cvImageNew);

            itemView.Click += (sender, e) => listener(base.LayoutPosition);
            itemView.SetOnCreateContextMenuListener(this);
        }

        public void SetActionView(ICardItem cardItem, string libraryPath) {
            this.cardItem = cardItem;

            //WARNING Inevitable abstraction leak due to Glide usage
            string fullCoverPath = System.IO.Path.Combine(libraryPath, cardItem.GetCoverPath());
            Glide.With(context).Load(fullCoverPath).FitCenter().Into(Cover);
            Caption.Text = cardItem.GetTitle();

            ProgressBar.Max = cardItem.GetPageCount();
            ProgressBar.Progress = cardItem.GetLastPageIndex();

            //If not album disable all overlay
            if (cardItem.GetCardType().Equals("query")) {
                ProgressBar.Visibility = ViewStates.Gone;
                New.Visibility = ViewStates.Gone;
                foreach(var flag in LanguageFlags) {
                    flag.Visibility = ViewStates.Gone;
                }
                return;
            }

            //Flag overlay
            for (int i = 0; i < languages.Count; i++) {
                LanguageFlags[i].Visibility = ViewStates.Gone;
                if (cardItem.GetLanguages().Contains(languages[i])) {
                    LanguageFlags[i].Visibility = ViewStates.Visible;
                }
            }

            //New album indicator overlay
            if (cardItem.GetIsNew()) {
                New.Visibility = ViewStates.Visible;
            }
            else {
                New.Visibility = ViewStates.Invisible;
            }
        }

        public void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo) {
            if(cardItem.GetCardType().Equals("album")) {
                menu.SetHeaderTitle("Album Menu");
                menu.Add(this.Position, 10, 0, "Delete Album");
                menu.Add(this.Position, 11, 0, "Edit Metadata");
            }
            else {
                menu.SetHeaderTitle("Query Menu");
                menu.Add(this.Position, 12, 0, "Quick Scan");
                menu.Add(this.Position, 13, 0, "Full Scan");
            }
            menu.Add(this.Position, 14, 0, "Save Library Db");
            menu.Add(this.Position, 15, 0, "DEBUG GetDb Db");
            menu.Add(this.Position, 16, 0, "TEST UndeleteAll");
        }
    }
}