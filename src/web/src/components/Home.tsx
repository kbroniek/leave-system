import {callApi} from "../utils/ApiCall"
export const Home = () => {
  callApi();
  return <p>You are on the home page</p>;
};
