import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Drawer, Modal, Button, Menu, Input, Dropdown } from 'antd';
import {
  EditOutlined,
  DeleteOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import { API_URL } from '../../Utilities/Config';
//import * as Helper from '../../Utilities/Helper';
import withMyAlert from '../../HOCs/withMyAlert';

const { confirm } = Modal;
const axios = require('axios').default;

function MyChapterDrawer(props) {
  const [chapters, setChapters] = useState([]);

  useEffect(() => {
    if (props.albumId === undefined) { return; }

    axios.get(API_URL + `Crud/GetAlbumChapters?id=${props.albumId}`)
      .then(function (response) {
        setChapters(response.data);
      })
      .catch(function (error) {
        props.popApiError(error);
      });

  }, [props.albumId]);

  const handler = {
    rename: (chapterTitle) => {
      console.log("rename ", chapterTitle);
    },
    delete: (chapterTitle) => {
      console.log("delete ", chapterTitle);
      confirm({
        title: `Delete chapter ${chapterTitle}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   Yes   ',
        okType: 'danger',
        cancelText: '   No   ',
        onOk() {
          let encodedChapterTitle = encodeURIComponent(chapterTitle);
          axios.delete(API_URL + `Crud/DeleteAlbumChapter/${props.albumId}/${encodedChapterTitle}`)
            .then(function (response) {
              console.log("newPageCount", response.data);
              props.onChapterDeleteSuccess(response.data);
              setChapters(
                chapters.filter(c => c.title !== chapterTitle)
              );
            })
            .catch(function (error) {
              props.popApiError(error);
            });
        },
        onCancel() {
        },
      });
    }
  };

  return (
    <Drawer
      placement="left"
      closable={false}
      onClose={props.onClose}
      visible={props.visible}
    >
      <div style={{ padding: "10px" }}>
        {chapters.map((chapter, index) => (
          <Row style={{ marginBottom: "5px" }} key={chapter.title}>
            <Dropdown overlay={
              <Menu>
                <Menu.Item key="1" onClick={() => handler.rename(chapter.title)}><EditOutlined />Rename</Menu.Item>
                <Menu.Item key="2" onClick={() => handler.delete(chapter.title)}><DeleteOutlined />Delete</Menu.Item>
              </Menu>}
              trigger={['contextMenu']}
            >
              <Col span={10} onClick={() => props.onJumpToPage(chapter.pageIndex)}>
                <img
                  style={{ objectFit: "contain", width: "100%", maxHeight: "128px", border: "1px solid white" }}
                  src={API_URL + "Media/StreamResizedImage?librelPathBase64=" + chapter.pageUncPath + "&maxSize=128"}
                  alt="img"
                >
                </img>
              </Col>
            </Dropdown>
            <Col span={14}>
              <span style={{ color: "white" }}>{chapter.title}</span>
            </Col>
          </Row>
        ))};
      </div>
    </Drawer>
  );
}

export default withMyAlert(MyChapterDrawer);