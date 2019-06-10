
/*
class CommentBox extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [] };
    }
    componentWillMount() {
        
        const xhr = new XMLHttpRequest();
        xhr.open('get', this.props.url, true);
        xhr.onload = () => {
            const data = JSON.parse(xhr.responseText);
            this.setState({ data: data });
        };
        xhr.send();
    }
    render() {
        return (
            <div className="commentBox">
                <CommentList data={this.state.data} />
            </div>
        );
    }
}
class CommentList extends React.Component {
    render() {
        const commentNodes = this.props.data.map(function (comment) {
            console.log(comment)
            return <Comment author={comment.author} key={comment.id} text={comment.text}/>
        });
        console.log(this.props.data);
        return <div className="commentList">{commentNodes}</div>;
    }
}
class Comment extends React.Component {
    render() {
        return (<div className="Comment">Author: {this.props.author} <br/>Text: {this.props.text}</div>)
    }
}
ReactDOM.render(<CommentBox url="/TestData" />, document.getElementById('reactcontent'));
*/