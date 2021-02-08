import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Drawer, Modal, Button, Menu, Input } from 'antd';
import { API_URL, IS_PUBLIC } from '../../Utilities/Config';
import * as Helper from '../../Utilities/Helper';
import { ArrowRightOutlined, DeleteOutlined } from '@ant-design/icons';
import MyReaderContextMenu from './MyReaderContextMenu';
import MyChapterDrawer from './MyChapterDrawer';
import loadingImg from '../../Resources/loading.gif';

import withMyAlert from '../../HOCs/withMyAlert';
import { useSwipeable } from 'react-swipeable';

const axios = require('axios').default;

function MyReaderModal(props) {
  console.log("MyReaderModal render");

  const [apm, setApm] = useState({ //apm = album pagination model
    id: null,
    lpi: 0,
    pages: [],
    indexes: {
      cPageI: 0,
      pPageI: 0,
      triad: "zenpen",
      zenpenIndex: 0,
      chuhenIndex: 0,
      kouhenIndex: 0,
    }
  });

  useEffect(() => {
    if (props.albumCm === undefined) { return; }
    if (props.albumCm.albumId === apm.id) { return; }

    axios.get(API_URL + "File/GetAlbumPageInfos?id=" + props.albumCm.albumId)
      .then(function (response) {
        setApm({
          id: props.albumCm.albumId,
          lpi: props.albumCm.lastPageIndex,
          pages: response.data,
          indexes: null
        });
      })
      .catch(function (error) {
        alert('GetAlbumPageInfos ' + error);
      });
  }, [props.albumCm]);

  useEffect(() => {
    const targetPage = apm.lpi < apm.pages.length ? apm.lpi : apm.pages.length - 1;

    pageHandler.jumpTo(targetPage);
  }, [apm.id]);

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown2, false);

    return () => { //component will unmount
      document.removeEventListener("keydown", handleKeyDown2, false);
    };
  }, []);

  const handleKeyDown2 = (event) => {
    if (event.keyCode === 27) {
      pageHandler.close();
    }
  };

  //#region Event handlers
  const pageHandler = {
    jumpTo: function (page, renameObj) {
      const maxIndex = apm.pages.length - 1;
      const newPPageI = apm.indexes?.cPageI ?? 0;
      const newCPageI = Helper.clamp(page, 0, maxIndex);

      const { newTriad, zenpenIndex, chuhenIndex, kouhenIndex } = (() => {
        function compose(newTriad, zenpenIndex, chuhenIndex, kouhenIndex) {
          return {
            newTriad,
            zenpenIndex,
            chuhenIndex,
            kouhenIndex
          };
        }

        function pageCeil(modifier) {
          const pagesLen = apm.pages.length;
          return (((newCPageI + modifier) % pagesLen) + pagesLen) % pagesLen;
        }

        if (newCPageI % 3 === 0) {
          return compose("zenpen", newCPageI, pageCeil(1), pageCeil(-1));
        }
        if (newCPageI % 3 === 1) {
          return compose("chuhen", pageCeil(-1), newCPageI, pageCeil(1));
        }
        if (newCPageI % 3 === 2) {
          return compose("kouhen", pageCeil(1), pageCeil(-1), newCPageI);
        }
      })();

      let modifiedPages = apm.pages;
      if (renameObj !== null && renameObj !== undefined) {
        modifiedPages[renameObj.index] = renameObj.newPageInfo;
      }

      setApm({
        indexes: {
          cPageI: newCPageI,
          pPageI: newPPageI,
          triad: newTriad,
          zenpenIndex: zenpenIndex,
          chuhenIndex: chuhenIndex,
          kouhenIndex: kouhenIndex
        },
        id: apm.id,
        lpi: apm.lpi,
        pages: modifiedPages,
      });

      if (apm.id != null && page === apm.pages.length - 1) {
        axios.post(API_URL + "Crud/UpdateAlbumOuterValue/", {
          albumId: apm.id,
          lastPageIndex: page
        })
          .then((response) => {
          })
          .catch((error) => {
            props.popApiError(error);
          });
      }
    },
    rename: (page, newName) => {
      console.log(`rename page ${page} with name ${newName}`);
      axios.post(API_URL + "File/RenameFile", {
        albumId: apm.id,
        pageIndex: page,
        newName: newName
      })
        .then(function (response) {
          const renameObj = {
            index: page,
            newPageInfo: response.data
          };
          console.log(renameObj);
          pageHandler.jumpTo(apm.indexes.cPageI - 1, renameObj);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    },
    delete: (page) => {
      axios.delete(API_URL + `File/DeleteFile/${apm.id}/${page}`)
        .then(function (response) {
          props.onPageDeleteSuccess(apm.id, page);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    },
    close: () => {
      props.onClose(apm.id, apm.indexes.cPageI);
    }
  }

  const albumHandler = {
    tierChange: (value) => {
      axios.put(API_URL + "Crud/UpdateAlbumTier", {
        albumId: apm.id,
        tier: value
      })
        .then(function (response) {
          console.log(response.data);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    },
    recount: () => {
      axios.post(API_URL + "Crud/RecountAlbumPages", {
        albumId: apm.id
      })
        .then(function (response) {
          console.log("new page count", response.data);
          props.onChapterDeleteSuccess(apm.id, response.data);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    }
  }

  const [showContextMenu, setShowContextMenu] = useState(false);
  const [showDrawer, setShowDrawer] = useState(false);

  const buttonHandler = {
    rightTop: () => {
      pageHandler.close();
    },
    rightMid: () => {
      pageHandler.jumpTo(apm.indexes.cPageI + 1);
    },
    rightBot: () => {
      pageHandler.jumpTo(apm.pages.length - 1);
    },
    midTop: () => {

    },
    midMid: () => {
      setShowContextMenu(true);
    },
    midBot: () => {

    },
    leftTop: () => {
      setShowDrawer(true);
    },
    leftMid: () => {
      pageHandler.jumpTo(apm.indexes.cPageI - 1);
    },
    leftBot: () => {
      pageHandler.jumpTo(0);
    }
  }

  const swipeHandler = useSwipeable({
    onSwipedLeft: (eventData) => { pageHandler.jumpTo(apm.indexes.cPageI + 1); },
    onSwipedRight: (eventData) => { pageHandler.jumpTo(apm.indexes.cPageI - 1); }
  });

  //#endregion
  const showGuide = IS_PUBLIC && apm.indexes !== null && apm.indexes.cPageI === 0;
  const overlayStyle = getOverlayStyle(showGuide);
  const buttonStyle = {
    side: {
      ...overlayStyle.button,
      minHeight: 0.25 * window.innerHeight
    },
    mid: {
      ...overlayStyle.button,
      minHeight: 0.5 * window.innerHeight
    }
  }

  if (apm.indexes === null || apm.pages.length === 0) { return (<></>); }

  const currentPageInfo = apm.indexes.triad === "zenpen" ? apm.pages[apm.indexes.zenpenIndex]
    : apm.indexes.triad === "chuhen" ? apm.pages[apm.indexes.chuhenIndex]
      : apm.indexes.triad === "kouhen" ? apm.pages[apm.indexes.kouhenIndex]
        : null;

  const zenpenPage = <PageDisplay
    id="zenpen-page"
    albumId={apm.id}
    pageInfo={apm.pages[apm.indexes.zenpenIndex]}
    queue={apm.indexes.triad === "zenpen" ? "q0" : apm.indexes.triad === "kouhen" ? "q1" : "q2"}
  />;

  const chuhenPage = <PageDisplay
    id="chuhen-page"
    albumId={apm.id}
    pageInfo={apm.pages[apm.indexes.chuhenIndex]}
    queue={apm.indexes.triad === "chuhen" ? "q0" : apm.indexes.triad === "zenpen" ? "q1" : "q2"}
  />;

  const kouhenPage = <PageDisplay
    id="kouhen-page"
    albumId={apm.id}
    pageInfo={apm.pages[apm.indexes.kouhenIndex]}
    queue={apm.indexes.triad === "kouhen" ? "q0" : apm.indexes.triad === "chuhen" ? "q1" : "q2"}
  />;

  return (
    <>
      <div className="overlay overlay-reader" style={{ display: props.isOpen ? "block" : "none", zIndex: 1 }}>
        <div className="overlay-content">
          {zenpenPage}
          {chuhenPage}
          {kouhenPage}
        </div>
        <div className="overlay" style={{ zIndex: 2 }}>
          <div className="overlay-content">
            <span className="with-shadow">{currentPageInfo.name}</span>
            <div style={{ position: "fixed", bottom: "0px", width: "100%", textAlign: "center" }}>
              <span className="with-shadow">{apm.indexes.cPageI + 1}/{apm.pages.length}</span>
            </div>
          </div>
        </div>
        <div className="overlay" style={{ zIndex: 3 }}>
          <div style={overlayStyle.content} {...swipeHandler}>
            <Row>
              <Col span={6}>
                <div onClick={buttonHandler.leftTop} style={buttonStyle.side}>
                  {getGuideLabel(showGuide, "Browse Chapters")}
                </div>
              </Col>
              <Col span={12}>
                <div onClick={buttonHandler.midTop} style={buttonStyle.side}>
                </div>
              </Col>
              <Col span={6}>
                <div onClick={buttonHandler.rightTop} style={buttonStyle.side}>
                  {getGuideLabel(showGuide, "Close Album")}
                </div>
              </Col>
            </Row>
            <Row>
              <Col span={6}>
                <div onClick={buttonHandler.leftMid} style={buttonStyle.mid}>
                  {getGuideLabel(showGuide, "Previous Page")}
                </div>
              </Col>
              <Col span={12}>
                <div onClick={buttonHandler.midMid} style={buttonStyle.mid}>
                  {getGuideLabel(showGuide, "Context Menu")}
                </div>
                <MyReaderContextMenu
                  albumCm={props.albumCm}
                  initialValue={apm.indexes.cPageI}
                  pageName={currentPageInfo.name}
                  onTierChange={albumHandler.tierChange}
                  onRecount={() => { albumHandler.recount(); setShowContextMenu(false); }}
                  onJump={(page) => { pageHandler.jumpTo(page); setShowContextMenu(false); }}
                  onUndoJump={() => { pageHandler.jumpTo(apm.indexes.pPageI); setShowContextMenu(false); }}
                  visible={showContextMenu}
                  onRename={(value) => { pageHandler.rename(apm.indexes.cPageI, value); setShowContextMenu(false); }}
                  onDelete={() => { pageHandler.delete(apm.indexes.cPageI); setShowContextMenu(false); }}
                  onCancel={() => setShowContextMenu(false)}
                />
              </Col>
              <Col span={6}>
                <div onClick={buttonHandler.rightMid} style={buttonStyle.mid}>
                  {getGuideLabel(showGuide, "Next Page")}
                </div>
              </Col>
            </Row>
            <Row>
              <Col span={6}>
                <div onClick={buttonHandler.leftBot} style={buttonStyle.side}>
                  {getGuideLabel(showGuide, "First Page")}
                </div>
              </Col>
              <Col span={12}>
                <div onClick={buttonHandler.midBot} style={buttonStyle.side} />
              </Col>
              <Col span={6}>
                <div onClick={buttonHandler.rightBot} style={buttonStyle.side}>
                  {getGuideLabel(showGuide, "Last Page")}
                </div>
              </Col>
            </Row>
          </div>
        </div>
      </div>
      <MyChapterDrawer
        visible={showDrawer}
        onClose={() => setShowDrawer(false)}
        albumId={apm.id}
        onJumpToPage={pageHandler.jumpTo}
        onChapterDeleteSuccess={(newPageCount) => props.onChapterDeleteSuccess(apm.id, newPageCount)}
      />
    </>
  );
}

const getOverlayStyle = (isGrey) => {
  const result = {
    content: {
      position: 'relative',
      top: '0%',
      width: '100%',
      textAlign: 'center',
      backgroundColor: isGrey ? 'rgba(200, 200, 200, 0.3)' : 'rgba(0, 0, 0, 0)'
    },
    button: {
      position: 'relative',
      boxSizing: 'border-box',
      border: isGrey ? '1px solid black' : '0px'
    }
  };

  return result;
}

const getGuideLabel = (isGrey, text) => {
  return isGrey ?
    <span style={{ textAlign: "center", margin: "auto", textShadow: "-1px 0 black, 0 1px black, 1px 0 black, 0 -1px black" }}>
      {text}
    </span> :
    "";
}

function PageDisplay(props) {
  const styleShow = { objectFit: "contain", height: window.innerHeight, width: window.innerWidth };
  const styleHide = { objectFit: "contain", height: 0, width: 0, position: "absolute" };
  //const styleShow = { height: 300, width: 300 };
  //const styleHide = { height: 150, width: 150 };

  if (IsVideo(props.pageInfo.extension)) {
    return (
      <video
        autoPlay loop
        id={props.id}
        style={props.queue === "q0" ? styleShow : styleHide}
        src={props.queue === "q0" ? API_URL + "Media/StreamPage?albumId=" + props.albumId + "&alRelPathBase64=" + props.pageInfo.uncPathEncoded : ""}
      >
        video not showing
      </video>
    )
  }

  return (
    <img
      id={props.id}
      style={props.queue === "q0" ? styleShow : styleHide}
      src={props.queue === "q0" || props.queue === "q1" ? API_URL + "Media/StreamPage?albumId=" + props.albumId + "&alRelPathBase64=" + props.pageInfo.uncPathEncoded : loadingImg}
      alt="Not loaded"
    />
  );
}

function IsVideo(extension) {
  if (extension === ".webm" || extension === ".mp4") {
    return true;
  }
  return false;
}

export default withMyAlert(MyReaderModal);