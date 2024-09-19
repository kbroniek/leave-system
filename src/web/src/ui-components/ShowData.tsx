export const ShowData = (props: {apiData: {[key: string]: string}}) => {
    return (
        <div><pre>{JSON.stringify(props.apiData, null, 2) }</pre></div>
    );
};

