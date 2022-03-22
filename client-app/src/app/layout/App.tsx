import React, { Fragment } from 'react';
import { Container } from 'semantic-ui-react';
import Navbar from './Navbar';
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard';
import { observer } from 'mobx-react-lite';
import HomePage from '../../features/home/HomePage';
import { Route, Routes, useLocation } from 'react-router';
import ActivityForm from '../../features/activities/form/ActivityForm';
import ActivityDetails from '../../features/activities/details/ActivityDetails';
import { ToastContainer } from 'react-toastify';
import NotFound from '../../features/errors/NotFound';
import TestErrors from '../../features/errors/TestErrors';
import ServerError from '../../features/errors/ServerError';

function App() {

  const location = useLocation();

  return (
    <Fragment>
      <ToastContainer position='bottom-right' hideProgressBar />
      <Container style={{ marginTop: '7em' }}>
        <Routes>
          <Route path='/' element={<HomePage />} />
          <Route element={<Navbar />}>
            <Route path='/activities' element={<ActivityDashboard />} />
            <Route path='/activities/:id' element={<ActivityDetails />} />
            {["/createActivity", "/manage/:id"].map((path) => {
              return (
                <Route key={location.key} path={path} element={<ActivityForm key={location.key} />} />
              );
            })}
            <Route path='/errors' element={<TestErrors />} />
            <Route path='/not-found' element={<NotFound />} />
            <Route path='/server-error' element={<ServerError />} />
          </Route>
        </Routes>
      </Container>
    </Fragment>
  );
}

export default observer(App);
