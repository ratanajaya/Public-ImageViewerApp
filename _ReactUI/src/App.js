import React from 'react';
import 'antd/dist/antd.css';
import './App.scss';
import { BrowserRouter, MemoryRouter, HashRouter, Route, Switch, Link, Redirect } from 'react-router-dom';

import MasterPage from './MasterPage';

function App() {
  return (
    <div className="App">
      <HashRouter>
        <Switch>
          <Route exact path="/" ><Redirect to="/genres" /></Route>
          <Route exact path="/albums" ><MasterPage page="Albums" /></Route>
          <Route exact path="/add" ><MasterPage page="AlbumAddEdit" /></Route>
          <Route exact path="/artists" ><MasterPage page="Artists" /></Route>
          <Route exact path="/genres" ><MasterPage page="Genres" /></Route>
        </Switch>
      </HashRouter>
    </div>
  );
}

export default App;
