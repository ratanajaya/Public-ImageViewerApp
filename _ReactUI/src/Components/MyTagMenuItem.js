import React, { useState } from 'react';
import { withRouter, Link } from "react-router-dom";
import { Row, Col } from 'antd';
import {
  PlusOutlined,
  MinusOutlined
} from '@ant-design/icons';

function MyTagMenuItem(props) {
  let encodedQuery = encodeURIComponent(props.tag.query);

  const plusBackground = props.selectStatus === "plus" ? "#1890ff" : "";
  const minusBackground = props.selectStatus === "minus" ? "#1890ff" : "";

  return (
    <li className="ant-menu-item" role="menuitem" style={{ paddingLeft: "24px", paddingRight: "0px" }}>
      <Row>
        <Col span={5} style={{ textAlign: "center" }} onClick={() => props.onPlus(props.tag.name)}><PlusOutlined style={{ marginRight: "0px", backgroundColor: plusBackground }} /></Col>
        <Link to={"/albums?query=" + encodedQuery} >
          <Col span={14}>
            <span style={{ fontSize: "12px" }}>{props.tag.name}</span>
          </Col>
        </Link>
        <Col span={5} style={{ textAlign: "center" }} onClick={() => props.onMinus(props.tag.name)}><MinusOutlined style={{ marginRight: "0px", backgroundColor: minusBackground }} /></Col>
      </Row>
    </li>
  );
}

export default MyTagMenuItem;