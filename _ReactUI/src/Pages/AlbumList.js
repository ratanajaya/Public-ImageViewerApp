import React, { useState, useEffect, useRef } from 'react';
import { API_URL } from '../Utilities/Config';

import MyAlbumCard from '../Components/Displays/MyAlbumCard';
import { Row, Col } from 'antd';
import MyReaderModal from '../Components/Displays/MyReaderModal';
//import * as Helper from '../Utilities/Helper';
import MyEditModal from '../Components/MyEditModal';

import withMyAlert from '../HOCs/withMyAlert';

const axios = require('axios').default;

function AlbumList(props) {
  //#region Display Album List
  const [albumCms, setAlbumCms] = useState([]);

  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  useEffect(() => {
    const encodedQuery = (props.query !== undefined) ? encodeURIComponent(props.query) : "";

    axios.get(API_URL + "Crud/GetAlbumCardModels?page=0&row=0&query=" + encodedQuery)
      .then((response) => {
        setAlbumCms(response.data);
      })
      .catch((error) => {
        props.popApiError(error);
      });

  }, [props.query]);
  //#endregion

  //#region Display Album Pages
  const [readerModalIsOpen, setReaderModalIsOpen] = useState(false);
  const [selectedAlbumId, setSelectedAlbumId] = useState({});

  const readerHandler = {
    view: (albumCm) => {
      setSelectedAlbumId(albumCm.albumId);
      setReaderModalIsOpen(true);
    },

    close: (albumId, lastPageIndex) => {
      setReaderModalIsOpen(false);

      axios.post(API_URL + "Crud/UpdateAlbumOuterValue/", {
        albumId: albumId,
        lastPageIndex: lastPageIndex
      })
        .then((response) => {
        })
        .catch((error) => {
          props.popApiError(error);
        });

      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.albumId !== albumId) {
          return albumCm;
        }
        return {
          ...albumCm,
          lastPageIndex: lastPageIndex,
          isRead: lastPageIndex === albumCm.pageCount - 1 ? true : albumCm.isRead
        };
      }));
    }
  }
  //#endregion

  //#region Edit Delete
  const [editModalIsOpen, setEditModalIsOpen] = useState(false);
  const [editedAlbumId, setEditedAlbumId] = useState(undefined);

  const crudHandler = {
    edit: (albumId) => {
      console.log("edit album" + albumId);
      setEditModalIsOpen(true);
      setEditedAlbumId(albumId);
    },
    delete: (albumId) => {
      setAlbumCms(albumCms.filter(albumCm => {
        return albumCm.albumId !== albumId;
      }));
    },
    pageDeleteSuccess: (albumId, page) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.albumId !== albumId) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: albumCm.pageCount - 1,
          lastPageIndex: page
        };
      }));
    },
    chapterDeleteSuccess: (albumId, newPageCount) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.albumId !== albumId) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: newPageCount,
          lastPageIndex: 0
        };
      }));
    }
  }

  const editHandler = {
    ok: (editedAlbumVm) => {
      setEditModalIsOpen(false);

      axios.post(API_URL + "Crud/UpdateAlbum/", editedAlbumVm)
        .then((response) => {
          console.log("edit success. Id: ", response.data);

          let newAlbumCms = [...albumCms];
          let editedAlbumVmIndex = newAlbumCms.findIndex(a => a.albumId === editedAlbumVm.albumId);
          newAlbumCms[editedAlbumVmIndex].fullTitle = `[${editedAlbumVm.album.artists.join(', ')}] ${editedAlbumVm.album.title}`;
          newAlbumCms[editedAlbumVmIndex].languages = editedAlbumVm.album.languages;
          setAlbumCms(newAlbumCms);
        })
        .catch((error) => {
          props.popApiError(error);
        });
    },
    cancel: () => {
      setEditModalIsOpen(false);
    }
  }

  //#endregion

  return (
    <>
      <Row gutter={0} type="flex">
        {albumCms.map((a, index) => (
          <Col style={{ textAlign: 'center' }} lg={3} md={6} xs={12} key={"albumCol" + a.albumId}>
            <MyAlbumCard
              index={index}
              albumCm={a}
              onView={readerHandler.view}
              onEdit={crudHandler.edit}
              onDelete={crudHandler.delete}
              showContextMenu={true}
            />
          </Col>
        ))}
      </Row>

      <MyReaderModal
        isOpen={readerModalIsOpen}
        onClose={readerHandler.close}
        onPageDeleteSuccess={crudHandler.pageDeleteSuccess}
        onChapterDeleteSuccess={crudHandler.chapterDeleteSuccess}
        albumCm={albumCms.find(aCm => aCm.albumId === selectedAlbumId)}
      />
      <MyEditModal
        isOpen={editModalIsOpen}
        albumId={editedAlbumId}
        onOk={editHandler.ok}
        onCancel={editHandler.cancel}
      />
    </>
  );
}

export default withMyAlert(AlbumList);