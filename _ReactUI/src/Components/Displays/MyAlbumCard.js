import React, { useState, useEffect } from 'react';
import { useInView } from 'react-intersection-observer';
import { API_URL } from '../../Utilities/Config';
import { Dropdown, Menu, Modal, Progress } from 'antd';
import Flags from 'country-flag-icons/react/3x2';
import { MoreOutlined, DeleteOutlined, EditOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import * as Helper from '../../Utilities/Helper';
import withMyAlert from '../../HOCs/withMyAlert';

const { confirm } = Modal;
const axios = require('axios').default;

function MyAlbumCard(props) {
  const handler = {
    view: function () {
      props.onView(props.albumCm)
    },

    edit: function () {
      props.onEdit(props.albumCm.albumId)
    },

    delete: function () {
      confirm({
        title: `Delete album ${props.albumCm.fullTitle}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   YES   ',
        okType: 'danger',
        cancelText: '   NO   ',
        onOk() {
          axios.delete(API_URL + "Crud/DeleteAlbum/" + props.albumCm.albumId)
            .then(function (response) {
              console.log("del success", response);
              props.onDelete(props.albumCm.albumId);
            })
            .catch(function (error) {
              props.popApiError(error);
            });
        },
        onCancel() {
        },
      });
    }
  }

  const menu = (
    <Menu>
      <Menu.Item key="1" onClick={handler.edit}><EditOutlined />Edit</Menu.Item>
      <Menu.Item key="2" onClick={handler.delete}><DeleteOutlined />Delete</Menu.Item>
    </Menu>
  );

  const [ref, inView, entry] = useInView({
    threshold: 0,
    rootMargin: "300px 0px 300px 0px"
  });

  const absoluteH = "150px";

  return (
    <div ref={ref} style={{ paddingBottom: '8px', position: "relative" }}>
      <Dropdown overlay={menu} trigger={['contextMenu']} disabled={!props.showContextMenu}>
        <div style={{ textAlign: "center" }}>
          <div style={{ height: absoluteH, width: "100%" }}>
            <div style={{
              display: "inline-block", position: "relative",
              height: absoluteH, width: "100%", maxWidth: absoluteH,
              borderLeft: !props.albumCm.isRead ? "1px solid #ddbe14" : "",
            }}>
              <img
                onClick={handler.view}
                style={{
                  objectFit: "contain",
                  height: "100%", width: "100%"
                }}
                src={inView ? API_URL + "Media/StreamResizedImage?librelPathBase64=" + props.albumCm.coverInfo.uncPathEncoded + "&maxSize=150" : ""}
                alt="img"
              />
              {!props.albumCm.isRead ?
                <div style={{
                  position: "absolute",
                  height: 0,
                  width: 0,
                  left: "0px",
                  top: "0px",
                  borderLeft: "15px solid #ddbe14",
                  borderRight: "15px solid transparent",
                  borderBottom: "15px solid transparent"
                }} /> : ""
              }
              <div style={{
                position: "absolute",
                right: "3px",
                bottom: "3px",
                color: "white",
                lineHeight: "0px"
              }}>
                {props.albumCm.languages.map((item, i) => <FlagIcon language={item} key={i} />)}
              </div>
              <div style={{
                position: "absolute",
                right: "0px",
                bottom: "0px",
                height: "100%",
                width: "2px"
              }}>
                {[3, 2, 1].map((e, i) =>
                  <div key={`tierBar-${i}`}
                    style={{
                      width: "100%",
                      height: "33%",
                      backgroundColor: ColorFromIndex(props.albumCm.tier, e),
                      borderTop: BorderFromIndex(props.albumCm.tier, e)
                    }} />
                )}
              </div>
              <div style={{
                position: "absolute",
                left: "0px",
                bottom: "0px",
                width: "100%",
                height: "3px",
                backgroundColor: "white",
                display: props.albumCm.lastPageIndex > 0 ? "block" : "none"
              }}>
                <div style={{
                  backgroundColor: "DodgerBlue",
                  height: "100%",
                  width: `${Helper.getPercent100(props.albumCm.lastPageIndex + 1, props.albumCm.pageCount)}%`
                }} />
              </div>
            </div>
          </div>
          <span style={{ fontSize: '12px', lineHeight: 1.1 }}>
            {props.albumCm.fullTitle}
          </span>
        </div>
      </Dropdown>
    </div>
  );
}

function ColorFromIndex(tier, e) {
  const top = { r: 34, g: 193, b: 195 };
  const mid = { r: 142, g: 190, b: 121 };
  const bot = { r: 253, g: 187, b: 45 };

  const rgb = e === 3 ? top : e === 2 ? mid : bot;
  const alpha = tier < e ? 0 : 1;

  const color = `rgba(${rgb.r},${rgb.g},${rgb.b},${alpha})`;

  return color;
}

function BorderFromIndex(tier, e) {
  return tier > e && (e === 1 || e === 2) ? "2px solid black" : "0px";
}

function FlagIcon(props) {
  const flagStyle = { width: "18px", height: "12px", marginRight: "2px", marginBottom: "2px" };

  return (
    <>
      {
        props.language === "English" ? <Flags.GB style={flagStyle} /> :
          props.language === "Japanese" ? <Flags.JP style={flagStyle} /> :
            props.language === "Chinese" ? <Flags.CN style={flagStyle} /> :
              <Flags.ID style={flagStyle} />
      }
    </>
  );
}

export default withMyAlert(MyAlbumCard);