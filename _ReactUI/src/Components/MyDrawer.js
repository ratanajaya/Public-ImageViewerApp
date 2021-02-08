import React, { useState } from 'react';
import ReactDOM from 'react-dom';
import { Drawer, Button } from 'antd';

function MyDrawer(props) {

  function handleClose() {
    props.onClose();
  }

  return (
    <Drawer
      placement="right"
      closable={false}
      onClose={handleClose}
      visible={props.showDrawer}
    >
      {props.content}
    </Drawer>
  );

}

export default MyDrawer;