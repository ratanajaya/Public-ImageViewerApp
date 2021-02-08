import React from 'react';
import { Button } from 'antd';
import { Row, Col } from 'antd';

function MyInputMulticheck(props) {

  function handleClick(label) {
    props.onChange(props.label, label);
  }

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={7}>
          <label>{props.label}</label>
        </Col>
        <Col span={17}>
          <Row gutter={2}>
            {props.items.map((item) => (
              <Col span={8} key={item}>
                <Button
                  key={item}
                  type={props.value.includes(item) ? "primary" : "link"}
                  ghost={props.value.includes(item) ? true : false}
                  onClick={() => handleClick(item)}
                  style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                >
                  {item}
                </Button>
              </Col>)
            )}
          </Row>
        </Col>
      </Row>
    </>
  );
}

export default MyInputMulticheck;