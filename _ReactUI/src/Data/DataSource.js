import { API_URL } from '../Utilities/Config';
import * as Helper from '../Utilities/Helper';

const axios = require('axios').default;

var albumInfo = null;

//#region AlbumInfo
export const GetAlbumInfo = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo;
}

export const GetTags = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo.tags;
}
export const GetCategories = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo.categories;
}
export const GetOrientations = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo.orientations;
}
export const GetLanguages = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo.languages;
}

async function FetchAlbumInfo() {
  console.log("FetchAlbumInfo called");
  const res = await fetch(API_URL + "Crud/GetAlbumInfo", {
    method: 'GET'
  });

  return res.json();
}
//#endregion

//#region Queries
var tagVMs = null;
export const GetTagVMs = async () => {
  if (tagVMs === null) {
    tagVMs = await FetchTagVMs();
  }
  return tagVMs;
}

async function FetchTagVMs() {
  const res = await fetch(API_URL + "Crud/GetTagVMs", {
    method: 'GET'
  });

  return res.json();
}
//#endregion

//#region AlbumViewer
export const GetAlbumVMs = (page, row, query) => {
  let encodedQuery = (typeof query !== 'undefined') ? encodeURIComponent(query) : "";
  let queryString = "?page=" + page + "&row=" + row + "&query=" + encodedQuery;

  axios.get(API_URL + "Crud/GetAlbums" + queryString)
    .then(function (response) {
      return response.data;
    })
    .catch(function (error) {
      Helper.apiErrorHandler(error);
    });

  // async function FetchAlbums() {
  //   const res = await fetch(API_URL + "Crud/GetAlbums" + queryString, {
  //     method: 'GET',
  //     //headers: '',
  //     mode: 'cors',
  //     cache: 'default'
  //   });
  //   return res.json();
  // };

  // let result = await FetchAlbums();
  // return result;
}
//#endregion

export const InsertAlbum = async (albumVm) => {
  const res = await fetch(API_URL + "Crud/InsertAlbum", {
    method: 'POST',
    body: JSON.stringify(albumVm),
    headers: {
      'Content-Type': 'application/json'
    }
  });
  return await res.json();
}