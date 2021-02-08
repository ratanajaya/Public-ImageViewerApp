import React from 'react';
import { Rate } from 'antd';
import { Row, Col } from 'antd';

function MyInputStar(props) {
  function handleChange(value) {
    props.onChange(props.label, value);
  }

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={7}>
          <label>{props.label}</label>
        </Col>
        <Col span={17}>
          <Rate
            count={3}
            value={props.value}
            onChange={handleChange}
          />
        </Col>
      </Row>
    </>
  );
}

export default MyInputStar;