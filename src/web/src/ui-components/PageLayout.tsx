import NavBar from "./NavBar";

export const PageLayout = (props: {children: JSX.Element | JSX.Element[]}) => {
    return (
        <>
            <NavBar />
            {props.children}
        </>
    );
};