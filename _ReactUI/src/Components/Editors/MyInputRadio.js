import React from 'react';
import { Radio } from 'antd';
import { Row, Col } from 'antd';

function MyInputRadio(props) {
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
          <Radio.Group onChange={handleChange} value={props.value} style={{ width: "100%" }}>
            <Row>
              {props.items.map(item => (
                <Col span={8} key={item}>
                  <Radio key={item} value={item}><span style={{ color: "white" }}>{item}</span></Radio>
                </Col>
              ))}
            </Row>
          </Radio.Group>
        </Col>
      </Row>
    </>
  );
}

export default MyInputRadio;