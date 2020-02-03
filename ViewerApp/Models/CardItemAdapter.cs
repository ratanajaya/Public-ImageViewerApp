using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Net;
using Android.Graphics;
using ViewerApp.AL;
using System.Collections.Generic;
using SharedLibrary;

namespace ViewerApp.Models
{
    public class CardItemAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        public List<ICardItem> CardItems { get; set; }

        string libraryPath;

        public CardItemAdapter(string libraryPath) {
            //WARNING Inevitable abstraction leak due to Glide usage in CardViewHolder
            this.libraryPath = libraryPath;
        }

        public CardItemAdapter(List<ICardItem> cardItems) { //UNUSED
            this.CardItems = cardItems;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PhotoCardView, parent, false);

            CardViewHolder vh = new CardViewHolder(parent.Context, itemView, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
            CardViewHolder vh = holder as CardViewHolder;

            vh.SetActionView(CardItems[position], libraryPath);
        }

        public override int ItemCount {
            get { return CardItems.Count; }
        }

        void OnClick(int position) {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}