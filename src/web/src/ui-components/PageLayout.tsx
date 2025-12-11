import NavBar from "./NavBar";
import React from "react";
export const PageLayout = (props: {
  children: React.ReactNode | React.ReactNode[];
}) => {
  return (
    <>
      <NavBar />
      {props.children}
    </>
  );
};
