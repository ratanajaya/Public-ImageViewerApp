import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Modal, Button } from 'antd';
import { API_URL } from '../Utilities/Config';
import * as Helper from '../Utilities/Helper';
import MyInputTextbox from './Editors/MyInputTextbox';
import MyInputStar from './Editors/MyInputStar';
import MyInputRadio from './Editors/MyInputRadio';
import MyInputCheck from './Editors/MyInputCheck';
import MyInputMulticheck from './Editors/MyInputMulticheck';

import withMyAlert from '../HOCs/withMyAlert';

import useSWR from 'swr';

//import * as DataSource from "../Data/DataSource";

const lodash = require('lodash/array');
const axios = require('axios').default;

function MyEditModal(props) {
  const { data: albumInfo, error: aiError } = useSWR("Crud/GetAlbumInfo");

  const [albumVm, setAlbumVm] = useState({
    albumId: "",
    path: "", //not updated
    pageCount: 0, //not updated
    lastPageIndex: 0, //not updated
    album: {
      title: "",
      category: "Manga",
      orientation: "Portrait",
      artists: [],
      tags: [],
      languages: [],
      flags: [],
      tier: 0,
      cover: "",
      isWip: false,
      isRead: false,
      entryDate: null
    }
  });

  useEffect(() => {
    if (props.albumId === undefined) { return; }

    axios.get(API_URL + "Crud/GetAlbumVm?id=" + props.albumId)
      .then(function (response) {
        console.log("GetAlbumVm response.data", response.data);
        setAlbumVm(response.data);
      })
      .catch(function (error) {
        props.popApiError(error);
      });

  }, [props.albumId]);

  const handlers = {
    albumVmChange: function (label, value) {
      let newAlbumVm = { ...albumVm };
      let cleanedValue = value;
      if (label === "Artists" || label === "Flags") {
        cleanedValue = value.split(",");
      }
      else if (label === "Tags" || label === "Languages") {
        cleanedValue = albumVm.album[Helper.firstLetterLowerCase(label)];
        if (cleanedValue.includes(value)) {
          cleanedValue = lodash.pull(cleanedValue, value);
        }
        else {
          cleanedValue.push(value);
        }
      }
      newAlbumVm.album[Helper.firstLetterLowerCase(label)] = cleanedValue;
      setAlbumVm(newAlbumVm);
    },
    ok: function () {
      props.onOk(albumVm);
    },
    cancel: function () {
      props.onCancel();
    }
  }

  return (
    <Modal
      title="Edit Metadata"
      visible={props.isOpen}
      onOk={handlers.ok}
      onCancel={handlers.cancel}
    >
      {aiError ? "album Info Fetching error" : !albumInfo ? "loading..." : (
        <>
          <MyInputTextbox label="Title" value={albumVm.album.title} onChange={handlers.albumVmChange} />
          <MyInputTextbox label="Artists" value={albumVm.album.artists.join()} onChange={handlers.albumVmChange} />
          <MyInputRadio label="Category" value={albumVm.album.category} items={albumInfo.categories} onChange={handlers.albumVmChange} />
          <MyInputRadio label="Orientation" value={albumVm.album.orientation} items={albumInfo.orientations} onChange={handlers.albumVmChange} />
          <MyInputStar label="Tier" value={albumVm.album.tier} onChange={handlers.albumVmChange} />
          <MyInputTextbox label="Flags" value={albumVm.album.flags.join()} onChange={handlers.albumVmChange} />
          <MyInputMulticheck label="Tags" value={albumVm.album.tags} items={albumInfo.tags} onChange={handlers.albumVmChange} />
          <MyInputMulticheck label="Languages" value={albumVm.album.languages} items={albumInfo.languages} onChange={handlers.albumVmChange} />
          <MyInputCheck label="IsRead" value={albumVm.album.isRead} onChange={handlers.albumVmChange} />
          <MyInputCheck label="IsWip" value={albumVm.album.isWip} onChange={handlers.albumVmChange} />
        </>
      )}
    </Modal>
  );
}

export default withMyAlert(MyEditModal);