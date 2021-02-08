import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Modal, Button, Menu, Input, Rate } from 'antd';
import { API_URL } from '../../Utilities/Config';
import * as Helper from '../../Utilities/Helper';
import { ArrowLeftOutlined, ArrowRightOutlined, DeleteOutlined, EditOutlined, ExclamationCircleOutlined, LineOutlined, FileSyncOutlined } from '@ant-design/icons';

//import './MyReaderModal.scss';

function MyReaderContextMenu(props) {
  const [jumpValue, setJumpValue] = useState(0);
  useEffect(() => {
    setJumpValue(props.initialValue + 1);
  }, [props.initialValue]);

  useEffect(() => {
    setNewPageName(props.pageName);
  }, [props.pageName]);

  const [showRenameModal, setShowRenameModal] = useState(false);
  const [showConfirmDelete, setShowConfirmDelete] = useState(false);

  const [tierDisplay, setTierDisplay] = useState(0);
  useEffect(() => {
    if (props.albumCm === undefined) { return; }
    setTierDisplay(props.albumCm.tier);
  }, [props.albumCm]);

  const [newPageName, setNewPageName] = useState("");

  if (props.albumCm === undefined) { return (<></>); }

  return (
    <>
      <Modal
        visible={props.visible}
        onCancel={props.onCancel}
        footer={null}
        closable={false}
        centered={true}
        style={{ maxWidth: 300 }}
      //bodyStyle={{ border: "1px solid black" }}
      >
        <Row>
          <Col span={24} style={{ marginBottom: 15, textAlign: "center" }}>
            <Rate
              count={3}
              value={tierDisplay}
              onChange={(value) => { setTierDisplay(value); props.onTierChange(value); }}
              style={{ fontSize: "40px" }}
            />
          </Col>
          <Col span={24}>
            <Button type="primary" onClick={() => props.onRecount()} style={{ width: "100%" }}>
              <FileSyncOutlined />Recount Pages
            </Button>
          </Col>
          <Col span={12} style={{ textAlign: "left", marginTop: 5 }}>
            <input type="number" value={jumpValue} onChange={(e) => setJumpValue(Math.min(Math.max(e.target.value, 1), props.albumCm.pageCount))} style={{ width: "97%", padding: "4px", WebkitBoxSizing: "border-box", borderRadius: "5px" }} />
          </Col>
          <Col span={12} style={{ textAlign: "right", marginTop: 5 }}>
            <Button type="primary" onClick={() => props.onJump(jumpValue - 1)} style={{ width: "97%", color: "white" }}>
              <ArrowRightOutlined />Jump
            </Button>
          </Col>
          <Col span={24} style={{ marginTop: 5 }}>
            <Button type="primary" onClick={() => props.onUndoJump()} style={{ width: "100%" }}>
              <ArrowLeftOutlined />Undo Jump
            </Button>
          </Col>
          <Col span={24} style={{ marginTop: 5 }}>
            <Button type="primary" onClick={() => { props.onCancel(); setShowRenameModal(true); }} style={{ width: "100%" }}>
              <EditOutlined />Rename Page
            </Button>
          </Col>
          <Col span={24} style={{ marginTop: 5 }}>
            <Button type="danger" onClick={() => { props.onCancel(); setShowConfirmDelete(true); }} style={{ width: "100%" }}>
              <DeleteOutlined />Delete Page
            </Button>
          </Col>
        </Row>
      </Modal>
      <Modal
        visible={showRenameModal}
        footer={null}
        centered={true}
        closable={false}
        onCancel={() => setShowRenameModal(false)}
        style={{ maxWidth: 300, textAlign: "center" }}
        bodyStyle={{ backgroundColor: "grey", border: "1px solid black" }}
      >
        <Input.TextArea value={newPageName} onChange={(event) => setNewPageName(event.target.value)} rows={2} />
        <Button type="primary" onClick={() => { setShowRenameModal(false); props.onRename(newPageName); }} style={{ width: "100%" }}>
          Rename
        </Button>
      </Modal>
      <Modal
        visible={showConfirmDelete}
        footer={null}
        centered={true}
        closable={false}
        onCancel={() => setShowConfirmDelete(false)}
        style={{ maxWidth: 300, textAlign: "center" }}
        bodyStyle={{ backgroundColor: "grey", border: "1px solid black" }}
      >
        <span style={{ color: "white" }}>Are you sure to delete this page?</span>
        <Button type="danger" onClick={() => { setShowConfirmDelete(false); props.onDelete(); }} style={{ width: "100%" }}>
          Delete
        </Button>
      </Modal>
    </>
  )
}

export default MyReaderContextMenu;