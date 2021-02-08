import React, { useState, useEffect, useRef } from 'react';

import MyAlbumCard from '../Components/Displays/MyAlbumCard';
import { Row, Col } from 'antd';
import useSWR from 'swr';
//first commit after github username changes

function GenreList(props) {
  const endpoint = props.page === "Genres" ? "Crud/GetGenreCardModels" :
    props.page === "Artists" ? "Crud/GetArtistCardModels?tier=1" :
      "NotFound";

  const { data: albumCGroups, error } = useSWR(endpoint);

  if (error) { return <div>error!</div>; }
  if (!albumCGroups) { return <div>loading...</div>; }

  const readerHandler = {
    view: function (albumCm) {
      console.log("query click", albumCm);
      props.history.push('/albums?query=' + albumCm.albumId)
    },
  }

  return (
    <>
      {albumCGroups.map((acg, index) => (
        <Row key={"acgRow" + acg.name} gutter={0} type="flex" style={{ border: "1px solid rgb(83, 83, 83)", borderRadius: "8px", paddingTop: "8px", marginBottom: "5px" }}>
          { props.page === "This feature is cancelled" ? (
            <div style={{ position: "relative" }}>
              <div style={{ position: "absolute", top: "-18px", left: "10px" }}>{acg.name}</div>
            </div>) : ""
          }
          {acg.albumCms.map((a) => (
            <Col style={{ textAlign: 'center' }} lg={3} md={6} xs={12} key={"albumCol" + a.albumId}>
              <MyAlbumCard
                index={index}
                albumCm={a}
                onView={readerHandler.view}
                onEdit={() => { }}
                onDelete={() => { }}
                showContextMenu={false}
              />
            </Col>
          ))}
        </Row>
      ))}
    </>
  );
}

export default GenreList;