import { Navigate, Route, RouteProps, useNavigate } from "react-router-dom";
import { useStore } from "../stores/store";

interface Props extends RouteProps {
    children: any;
    //path?: string;
    //roles
}

export default function PrivateRoute({ children: Component }: Props) {
    const { userStore: { isLoggedIn } } = useStore();

    if (isLoggedIn) {
        return Component;
    }

    return <Navigate to="/" />;
}