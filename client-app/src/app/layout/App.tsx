import { Fragment, useEffect } from "react";
import { Container } from "semantic-ui-react";
import { observer } from "mobx-react-lite";
import { Outlet } from "react-router";
import { ToastContainer } from "react-toastify";
import { useStore } from "../stores/store";
import LoadingComponent from "./LoadingComponent";
import ModalContainer from "../common/modals/ModalContainer";
import { useLocation } from "react-router-dom";
import HomePage from "../../features/home/HomePage";
import Navbar from "./Navbar";

function App() {
  const location = useLocation();
  const { commonStore, userStore } = useStore();

  useEffect(() => {
    if (commonStore.token) {
      userStore.getUser().finally(() => commonStore.setAppLoaded());
    } else {
      userStore.getFacebookLoginStatus().then(() => commonStore.setAppLoaded());
    }
  }, [commonStore, userStore]);

  if (!commonStore.appLoaded) return <LoadingComponent content="Loading..." />;

  return (
    <Fragment>
      <ToastContainer position="bottom-right" hideProgressBar />
      <ModalContainer />
      {location.pathname === "/" ? (
        <HomePage />
      ) : (
        <>
          <Navbar />
          <Container style={{ marginTop: "7em" }}>
            <Outlet />
          </Container>
        </>
      )}
    </Fragment>
  );
}

export default observer(App);

// {/* <Routes>
//   <Route path='/' element={<HomePage />} />
//   <Route element={<Navbar />}>
//     <Route path='/activities' element={<PrivateRoute children={<ActivityDashboard />} />} />
//     <Route path='/activities/:id' element={<PrivateRoute children={<ActivityDetails />} />} />
//     {/* {["/createActivity", "/manage/:id"].map((path) => {
//       return (
//         <Route key={location.key} path={path} element={<ActivityForm key={location.key} />} />
//       );
//     })} */}
//     {["/createActivity", "/manage/:id"].map((path) => {
//       return (
//         <Route key={location.key} path={path} element={<PrivateRoute children={<ActivityForm key={location.key} />} />} />
//       );
//     })}
//     <Route path='/profiles/:username' element={<PrivateRoute children={<ProfilePage />} />} />
//     <Route path='/errors' element={<PrivateRoute children={<TestErrors />} />} />
//     <Route path='/server-error' element={<PrivateRoute children={<ServerError />} />} />
//     <Route path='/account/registerSuccess' element={<RegisterSuccess />}/>
//     <Route path='/account/verifyEmail' element={<ConfirmEmail />}/>
//     {/* <Route path='/not-found' element={<NotFound />} /> */}
//   </Route>
// </Routes> */}
