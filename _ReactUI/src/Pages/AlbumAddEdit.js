import React, { useState, useEffect } from 'react';
import { withRouter } from "react-router-dom";
import MyInputTextbox from '../Components/Editors/MyInputTextbox';
import { Button, Row, Col, Upload } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import MyInputRadio from '../Components/Editors/MyInputRadio';
import MyInputMulticheck from '../Components/Editors/MyInputMulticheck';
import MyInputCheck from '../Components/Editors/MyInputCheck';
import MyInputStar from '../Components/Editors/MyInputStar';

import * as DataSource from "../Data/DataSource";
import * as Helper from "../Utilities/Helper";
import MyInputButton from '../Components/Editors/MyInputButton';

var lodash = require('lodash/array');


function AlbumAddEdit(props) {
  const [FolderName, setFolderName] = useState("");
  const [Files, setFiles] = useState([]);
  const [AlbumVm, setAlbumVm] = useState({
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
  });

  const [Tags, setTags] = useState([]);
  const [Categories, setCategories] = useState([]);
  const [Orientations, setOrientations] = useState([]);
  const [Languages, setLanguages] = useState([]);

  var webkit;

  useEffect(() => {
    async function FetchAlbumInfo() {
      let _tag = await DataSource.GetTags();
      let _categories = await DataSource.GetCategories();
      let _orientations = await DataSource.GetOrientations();
      let _languages = await DataSource.GetLanguages();
      setTags(_tag);
      setCategories(_categories);
      setOrientations(_orientations);
      setLanguages(_languages);
    }
    FetchAlbumInfo();

    webkit = document.getElementById("myInput");
    webkit.onchange = function () {
      console.log(webkit);
      setFiles(webkit.files);
      // var files = webkit.files,
      //   len = files.length,
      //   i;
      // for (i = 0; i < len; i += 1) {
      //   //console.log(files[i]);
      // }
    };
  }, []);

  function handleAlbumVmChange(label, value) {
    let newAlbumVm = { ...AlbumVm };
    let cleanedValue = value;
    if (label === "Artists" || label === "Flags") {
      cleanedValue = value.split(",");
    }
    else if (label === "Tags" || label === "Languages") {
      cleanedValue = AlbumVm[Helper.firstLetterLowerCase(label)];
      if (cleanedValue.includes(value)) {
        cleanedValue = lodash.pull(cleanedValue, value);
      }
      else {
        cleanedValue.push(value);
      }
    }
    newAlbumVm[Helper.firstLetterLowerCase(label)] = cleanedValue;
    setAlbumVm(newAlbumVm);
  }

  function handleLogAlbum(event) {
    console.log(AlbumVm);
  }
  function handleSubmit(event) {
    console.log(event);
  }

  function logWebkitFiles() {
    // for (let i = 0; i < Files.length; i++) {
    //   var reader = new FileReader();
    //   reader.onload = function (e) {
    //     var object = new Object();
    //     object.content = e.target.content;
    //     var json_upload = "jsonObject=" + JSON.stringify(object);

    //     console.log("what is e", e);
    //     console.log("what is json_u", json_upload);
    //   }
    //   reader.readAsBinaryString(Files);
    // }
    console.log(Files[0]);
  }

  return (
    <div>
      <h3>Add edit page</h3>
      <br />
      <Row justify="start" align="middle" gutter={[16, 24]}>
        <Col span={12} className="gutter-row">
          <MyInputTextbox label="Title" value={AlbumVm.title} onChange={handleAlbumVmChange} />
          <MyInputTextbox label="Artists" value={AlbumVm.artists.join()} onChange={handleAlbumVmChange} />
          <MyInputRadio label="Category" value={AlbumVm.category} items={Categories} onChange={handleAlbumVmChange} />
          <MyInputStar label="Tier" value={AlbumVm.tier} onChange={handleAlbumVmChange} />
          <MyInputTextbox label="Flags" value={AlbumVm.flags.join()} onChange={handleAlbumVmChange} />
          <MyInputMulticheck label="Tags" value={AlbumVm.tags} items={Tags} onChange={handleAlbumVmChange} />
          <MyInputRadio label="Orientation" value={AlbumVm.orientation} items={Orientations} onChange={handleAlbumVmChange} />
          <MyInputMulticheck label="Languages" value={AlbumVm.languages} items={Languages} onChange={handleAlbumVmChange} />
          <MyInputCheck label="IsRead" value={AlbumVm.isRead} onChange={handleAlbumVmChange} />
          <MyInputCheck label="IsWip" value={AlbumVm.isWip} onChange={handleAlbumVmChange} />
          <Button onClick={() => logWebkitFiles()}>Log Webkit Files</Button>
          <MyInputButton label="Insert Album" onClick={handleSubmit} />
          {/* <Upload directory>
            <Button>
              <UploadOutlined /> Select Folder
            </Button>
          </Upload> */}
          <input id="myInput" type="file" webkitdirectory="true" directory multiple />
        </Col>
        <Col span={12} className="gutter-row">
          <p>placeholder for image</p>
        </Col>
      </Row>
    </div>
  );
}

export default AlbumAddEdit;