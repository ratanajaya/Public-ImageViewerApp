import React, { useState, useEffect } from 'react';
import { withRouter, Link } from "react-router-dom";
import queryString from 'query-string';
import { API_URL } from './Utilities/Config';
import { DesktopOnly, MobileOnly } from './Utilities/Display';

import { Layout, Menu, Drawer, Button, Row, Col, Input } from 'antd';
import {
  DesktopOutlined,
  TeamOutlined,
  PartitionOutlined,
  TagsOutlined,
  ReloadOutlined,
  SyncOutlined,
  DatabaseOutlined
} from '@ant-design/icons';

import MenuIcon from '@material-ui/icons/Menu';

import AlbumList from './Pages/AlbumList';
import AlbumAddEdit from './Pages/AlbumAddEdit';
import GenreList from './Pages/GenreList';
import MyTagMenuItem from './Components/MyTagMenuItem';

import * as Helper from "./Utilities/Helper";
import withMyAlert from "./HOCs/withMyAlert";
import MyQueryEditor from './Components/Editors/MyQueryEditor';

import useSWR, { SWRConfig } from 'swr';

const axios = require('axios').default;

const { Header, Content, Footer, Sider } = Layout;
const { SubMenu } = Menu;

//const fetcher = (endpoint) => fetch(API_URL + endpoint).then(res => res.json());

function MasterPage(props) {
  const [showDrawer, setShowDrawer] = useState(false);
  const [collapseSider, setCollapseSider] = useState(false);

  let querStr = queryString.parse(props.location.search).query ?? "";

  let page;
  let selectedMenu;
  if (props.page === "Albums") {
    page = <AlbumList query={querStr} />;
    selectedMenu = '2';
  }
  else if (props.page === "AlbumAddEdit") {
    page = <AlbumAddEdit />;
    selectedMenu = '1';
  }
  else if (props.page === "Artists") {
    page = <GenreList page={props.page} history={props.history} />;
    selectedMenu = '3';
  }
  else if (props.page === "Genres") {
    page = <GenreList page={props.page} history={props.history} />;
    selectedMenu = '4';
  }

  const queryHandler = {
    search: function (query) {
      let correctedQuery = Helper.contains(query.split(",")[0], [":", "!", ">", "<"]) ? query : 'title:' + query;
      queryHandler.goToAlbumList(correctedQuery);
    },
    plus: function (tagName) {
      const targetQueryPlus = "tag:" + tagName;
      const targetQueryMinus = "tag!" + tagName;

      let currentQueries = querStr !== "" ? querStr.split(',') : [];
      let newQueries = currentQueries.filter((item, index) => !(item === targetQueryPlus || item === targetQueryMinus));
      newQueries.push(targetQueryPlus);
      let newQueryString = newQueries.join(',');

      queryHandler.goToAlbumList(newQueryString);
    },
    minus: function (tagName) {
      const targetQueryPlus = "tag:" + tagName;
      const targetQueryMinus = "tag!" + tagName;

      let currentQueries = querStr !== "" ? querStr.split(',') : [];
      let newQueries = currentQueries.filter((item, index) => !(item === targetQueryPlus || item === targetQueryMinus));
      newQueries.push(targetQueryMinus);
      let newQueryString = newQueries.join(',');

      queryHandler.goToAlbumList(newQueryString);
    },
    goToAlbumList: function (query) {
      let currentUrlParams = new URLSearchParams(window.location.search);
      currentUrlParams.set('query', query);
      props.history.push("Albums?" + currentUrlParams.toString());
    }
  }

  const actionHandler = {
    reload: () => {
      axios.get(API_URL + "Crud/ReloadDatabase")
        .then((response) => {
          props.popInfo("ReloadDatabase", response.data);
        })
        .catch((error) => {
          props.popApiError(error);
        })
    },
    rescan: () => {
      axios.get(API_URL + "Crud/RescanDatabase")
        .then((response) => {
          props.popInfo("RescanDatabase", response.data);
        })
        .catch((error) => {
          props.popApiError(error);
        })
    }
  }

  const menuContent = (
    <>
      <div className="logo"></div>
      <MyQueryEditor query={querStr} onOk={(query) => queryHandler.goToAlbumList(query)} />
      <Menu theme="dark" defaultSelectedKeys={[selectedMenu]} defaultOpenKeys={["sub1"]} mode="inline">
        <Menu.Item key="2">
          <Link to="/albums">
            <DesktopOutlined />
            <span>Albums</span>
          </Link>
        </Menu.Item>
        <Menu.Item key="3">
          <Link to="/artists">
            <TeamOutlined />
            <span>Artists</span>
          </Link>
        </Menu.Item>
        <Menu.Item key="4">
          <Link to="/genres">
            <PartitionOutlined />
            <span>Genres</span>
          </Link>
        </Menu.Item>
        <SubMenu key="sub1" title={<><TagsOutlined /><span>Tags</span></>} >
          <TagItemsContainer onPlus={queryHandler.plus} onMinus={queryHandler.minus} querStr={querStr} />
        </SubMenu>
        <SubMenu key="sub2" title={<><DatabaseOutlined /><span>Actions</span></>} >
          <li className="ant-menu-item" role="menuitem" style={{ paddingLeft: "24px", paddingRight: "0px" }}>
            <Row onClick={actionHandler.reload}>
              <Col span={5} style={{ textAlign: "center" }}><ReloadOutlined style={{ marginRight: "0px" }} /></Col>
              <Col span={14}>
                <span style={{ fontSize: "12px" }}>Reload Db</span>
              </Col>
              <Col span={5} style={{ textAlign: "center" }}></Col>
            </Row>
          </li>
          <li className="ant-menu-item" role="menuitem" style={{ paddingLeft: "24px", paddingRight: "0px" }}>
            <Row onClick={actionHandler.rescan}>
              <Col span={5} style={{ textAlign: "center" }}><SyncOutlined style={{ marginRight: "0px" }} /></Col>
              <Col span={14}>
                <span style={{ fontSize: "12px" }}>Rescan Db</span>
              </Col>
              <Col span={5} style={{ textAlign: "center" }}></Col>
            </Row>
          </li>
        </SubMenu>
      </Menu>
    </>
  );

  return (
    <SWRConfig value={{ revalidateOnFocus: false, fetcher: (endpoint) => fetch(API_URL + endpoint).then(res => res.json()) }}>
      <Layout style={{ minHeight: '100vh' }}>
        <MobileOnly>
          <Layout style={{ position: 'fixed', zIndex: 1, width: '100%', backgroundColor: '#001529' }}>
            <div style={{ textAlign: "end", lineHeight: 0 }}>
              <div className="hamburger-container" onClick={() => setShowDrawer(true)}>
                <MenuIcon style={{ fontSize: '32px' }} />
              </div>
            </div>
          </Layout>
          <Drawer
            placement="right"
            closable={false}
            onClose={() => setShowDrawer(false)}
            visible={showDrawer}
          >
            {menuContent}
          </Drawer>
        </MobileOnly>
        <DesktopOnly>
          <Sider collapsible collapsed={collapseSider} onCollapse={(event) => setCollapseSider(event)}>
            {menuContent}
          </Sider>
        </DesktopOnly>

        <Layout className="site-layout">
          <Content>
            <MobileOnly>
              <div style={{ paddingTop: "5x" }}>Padding for mobile</div>
            </MobileOnly>
            <div className="site-layout-background" style={{ paddingTop: 36, paddingLeft: 10, paddingRight: 10, minHeight: 360 }}>
              {page}
            </div>
          </Content>
          <Footer style={{ textAlign: 'center' }}>-----------------</Footer>
        </Layout>
      </Layout>
    </SWRConfig>
  );
}

export default withRouter(withMyAlert(MasterPage));

function TagItemsContainer(props) {
  const { data: tags, error } = useSWR("Crud/GetTagVMs");

  if (error) { return <div>error!</div>; }
  if (!tags) { return <div>loading...</div>; }

  return (
    <>
      {tags.map((tag, index) => (<MyTagMenuItem key={tag.name} tag={tag} onPlus={props.onPlus} onMinus={props.onMinus} selectStatus={getSelectStatus(tag.name, props.querStr)} />))}
    </>
  )
}

function getSelectStatus(tagName, query) {
  let queryArr = query.split(',');
  let result = queryArr.filter((str) => str.toLowerCase().includes("tag:" + tagName.toLowerCase())).length > 0 ? "plus" :
    queryArr.filter((str) => str.toLowerCase().includes("tag!" + tagName.toLowerCase())).length > 0 ? "minus" :
      "none";

  return result;
}