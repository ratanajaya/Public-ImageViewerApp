import React from 'react';
import { Button, Row, Col } from 'antd';

function MyInputButton(props) {
  function handleClick(event) {
    props.onClick(props.label);
  }

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={7}>
        </Col>
        <Col span={17}>
          <Button onClick={handleClick}>{props.label}</Button>
        </Col>
      </Row>
    </>
  );
}

export default MyInputButton;