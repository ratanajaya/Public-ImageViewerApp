import React, { useState, useEffect } from 'react';
import { Row, Col, Modal, Input, Button } from 'antd';
import {
  ConsoleSqlOutlined,
  SearchOutlined
} from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';

function MyQueryEditor(props) {
  const [visible, setVisible] = useState(false);
  const [query, setQuery] = useState("");

  useEffect(() => {
    setQuery(props.query);
  }, [props.query]);

  function handleOk() {
    setVisible(false);
    props.onOk(query);
  }

  return (
    <div style={{ paddingLeft: "24px" }}>
      <Button type="link" onClick={() => setVisible(true)} style={{ width: "90%", textAlign: "left", padding: "0px" }}>
        <ConsoleSqlOutlined />Query Editor
      </Button>
      <Modal
        visible={visible}
        onCancel={() => setVisible(false)}
        closable={false}
        footer={
          <Button type="primary" onClick={handleOk}>
            <SearchOutlined />Search
          </Button>
        }
      >
        <TextArea onChange={(event) => setQuery(event.target.value)} value={query} autoSize={{ minRows: 2, maxRows: 5 }} />
      </Modal>
    </div>
  );
}

export default MyQueryEditor;