import React from 'react';
import { Input } from 'antd';
import { Row, Col } from 'antd';

function MyInputTextbox(props) {
  function handleChange(event) {
    props.onChange(props.label, event.target.value);
  }

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={7}>
          <label>{props.label}</label>
        </Col>
        <Col span={17}>
          <Input
            placeholder={props.placeholder}
            value={props.value}
            onChange={handleChange}
          />
        </Col>
      </Row>
    </>
  );
}

export default MyInputTextbox;