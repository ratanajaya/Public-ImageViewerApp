import React from 'react';
import { Checkbox } from 'antd';
import { Row, Col } from 'antd';

function MyInputCheck(props) {
  function handleChange(event) {
    props.onChange(props.label, event.target.checked);
  }

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={7}>
          <label>{props.label}</label>
        </Col>
        <Col span={17}>
          <Checkbox onChange={handleChange} checked={props.value} />
        </Col>
      </Row>
    </>
  );
}

export default MyInputCheck;