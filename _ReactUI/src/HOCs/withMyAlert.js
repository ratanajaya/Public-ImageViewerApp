import React from 'react';
import { Modal } from 'antd';

const withMyAlert = OriginalComponent => ({ ...props }) => {
  const width = 500;

  function popInfo(title, stringContent, objContent) {
    Modal.info({
      title: (title),
      content: (
        <span>
          {stringContent}
          {JSON.stringify(objContent, null, 2)}
        </span>
      ),
      onOk() { },
      width: width
    });
  }

  function popError(title, stringContent, objContent) {
    Modal.error({
      title: (title),
      content: (
        <span>
          {stringContent}
          {JSON.stringify(objContent, null, 2)}
        </span>
      ),
      onOk() { },
      width: width
    });
  }

  function popApiError(error) {
    if (error.response == null) {
      Modal.error(JSON.stringify(error));
      return;
    }
    const { status, statusText, data } = error.response;

    const content = {
      title: `${status} - ${statusText}`,
      content: (
        <span>
          {JSON.stringify(data, null, 2)}
        </span>
      ),
      onOk() { },
      width: width
    };

    if (status >= 400 && status < 500) {
      Modal.warning(content);
    }
    else {
      Modal.error(content);
    }
  }

  return (
    <OriginalComponent
      {...props}
      popInfo={popInfo}
      popError={popError}
      popApiError={popApiError}
    />
  )
}

export default withMyAlert;