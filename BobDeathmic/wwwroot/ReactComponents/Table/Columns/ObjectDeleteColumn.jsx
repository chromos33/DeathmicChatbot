class ObjectDeleteColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleDeleteClick = this.handleDeleteClick.bind(this);
        this.handleCancelClick = this.handleCancelClick.bind(this);
        this.handleToggleDeleteForm = this.handleToggleDeleteForm.bind(this);
        this.state = {
            Open: false
        };
    }
    handleCancelClick(e) {
        this.setState({ Open: false });
    }
    handleDeleteClick(e) {

        const xhr = new XMLHttpRequest();
        var thisreference = this;
        xhr.open('GET', this.props.data.DeleteLink, true);
        xhr.onload = function () {
            if (xhr.responseText !== false) {
                thisreference.setState({ Open: false });
                window.dispatchEvent(new Event('updateTable'));
            }
        };
        xhr.send();

    }
    handleToggleDeleteForm(e) {
        this.setState({ Open: true });
    }
    render() {
        if (this.props.data !== undefined) {
            if (this.state.Open) {
                return <td>
                    {this.props.data.Text}
                    <div className="shadowlayer"></div>
                    <div className="statictest grid column-4 row-3">
                        <h1 className="deleteText">{this.props.data.DeleteText}</h1>
                        <span onClick={this.handleDeleteClick} className="btn btn_primary deletebtn">Endgültig löschen</span>
                        <span onClick={this.handleCancel} className="btn btn_primary canceldeletebtn">Abbrechen</span>
                    </div>
                </td>;
            }
            else {
                return <td className="pointer" onClick={this.handleToggleDeleteForm}>
                    {this.props.data.Text}
                </td>;
            }
        } else {
            return null;
        }



    }
}
