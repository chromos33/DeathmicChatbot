"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return right[Symbol.hasInstance](left); } else { return left instanceof right; } }

function _typeof(obj) { if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!_instanceof(instance, Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

var ChatUserSelect =
    /*#__PURE__*/
    function (_React$Component) {
        _inherits(ChatUserSelect, _React$Component);

        function ChatUserSelect(props) {
            var _this;

            _classCallCheck(this, ChatUserSelect);

            _this = _possibleConstructorReturn(this, _getPrototypeOf(ChatUserSelect).call(this, props));
            _this.state = {
                chatUsers: [],
                selectedUser: ""
            };
            _this.handleOnClick = _this.handleOnClick.bind(_assertThisInitialized(_this));
            _this.handleOnChange = _this.handleOnChange.bind(_assertThisInitialized(_this));
            return _this;
        }

        _createClass(ChatUserSelect, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                $.ajax({
                    url: "/Events/InvitableUsers/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        thisreference.setState({
                            chatUsers: result,
                            selectedUser: result[0].name
                        });
                    }
                });
            }
        }, {
            key: "handleOnClick",
            value: function handleOnClick(event) {
                var thisreference = this;
                $.ajax({
                    url: "/Events/AddInvitedUser/",
                    type: "POST",
                    data: {
                        ID: thisreference.props.ID,
                        ChatUser: thisreference.state.selectedUser
                    },
                    success: function success(result) {
                        thisreference.props.eventEmitter.emitEvent("UpdateChatMembers");
                    }
                });
            }
        }, {
            key: "handleOnChange",
            value: function handleOnChange(event) {
                console.log(event.target.value);
                this.setState({
                    selectedUser: event.target.value
                });
            }
        }, {
            key: "render",
            value: function render() {
                if (this.state.chatUsers.length > 0) {
                    var chatUserNodes = this.state.chatUsers.map(function (chatUser) {
                        return React.createElement("option", {
                            key: chatUser.name,
                            value: chatUser.name
                        }, chatUser.name);
                    });
                    return React.createElement("div", null, React.createElement("select", {
                        key: this.props.key,
                        value: this.state.selectedUser,
                        onChange: this.handleOnChange,
                        className: "chatUser_" + this.props.key
                    }, chatUserNodes), React.createElement("span", {
                        className: "button",
                        onClick: this.handleOnClick
                    }, "Invite"));
                }

                return React.createElement("p", null, " No Users Loaded");
            }
        }]);

        return ChatUserSelect;
    }(React.Component);

var EditEvent =
    /*#__PURE__*/
    function (_React$Component2) {
        _inherits(EditEvent, _React$Component2);

        function EditEvent(props) {
            var _this2;

            _classCallCheck(this, EditEvent);

            _this2 = _possibleConstructorReturn(this, _getPrototypeOf(EditEvent).call(this, props));
            _this2.state = {
                data: [],
                eventEmitter: new EventEmitter()
            };
            return _this2;
        }

        _createClass(EditEvent, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                $.ajax({
                    url: "/Events/GetEvent/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        thisreference.setState({
                            data: result
                        });
                    }
                });
            }
        }, {
            key: "render",
            value: function render() {
                if (this.state.data.name === undefined) {
                    return React.createElement("div", {
                        className: "OverView"
                    }, React.createElement(NameField, {
                        owner: this.props.ID,
                        value: ""
                    }));
                } else {
                    return React.createElement("div", {
                        className: "OverView"
                    }, React.createElement("span", null, "Name"), React.createElement(NameField, {
                        owner: this.props.ID,
                        value: this.state.data.name
                    }), React.createElement(ChatUserSelect, {
                        ID: this.props.ID,
                        eventEmitter: this.state.eventEmitter
                    }), React.createElement(InvitedUserList, {
                        ID: this.props.ID,
                        eventEmitter: this.state.eventEmitter
                    }), React.createElement(TemplateList, {
                        ID: this.props.ID,
                        eventEmitter: this.state.eventEmitter
                    }));
                }
            }
        }]);

        return EditEvent;
    }(React.Component);

var InvitedUserList =
    /*#__PURE__*/
    function (_React$Component3) {
        _inherits(InvitedUserList, _React$Component3);

        function InvitedUserList(props) {
            var _this3;

            _classCallCheck(this, InvitedUserList);

            _this3 = _possibleConstructorReturn(this, _getPrototypeOf(InvitedUserList).call(this, props));
            _this3.state = {
                InvitedUsers: []
            };
            _this3.handleUpdateChatMembers = _this3.handleUpdateChatMembers.bind(_assertThisInitialized(_this3));
            _this3.handleOnRemoveClick = _this3.handleOnRemoveClick.bind(_assertThisInitialized(_this3));
            return _this3;
        }

        _createClass(InvitedUserList, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                $.ajax({
                    url: "/Events/InvitedUsers/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        thisreference.setState({
                            InvitedUsers: result
                        });
                    }
                });
                this.props.eventEmitter.addListener("UpdateChatMembers", thisreference.handleUpdateChatMembers);
            }
        }, {
            key: "handleUpdateChatMembers",
            value: function handleUpdateChatMembers(event) {
                var thisreference = this;
                $.ajax({
                    url: "/Events/InvitedUsers/" + thisreference.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        thisreference.setState({
                            InvitedUsers: result
                        });
                    }
                });
            }
        }, {
            key: "handleOnRemoveClick",
            value: function handleOnRemoveClick(event) {
                var thisreference = this;
                event.persist();
                $.ajax({
                    url: "/Events/RemoveInvitedUser/",
                    type: "POST",
                    data: {
                        ID: thisreference.props.ID,
                        ChatUser: event.target.dataset.value
                    },
                    success: function success(result) {
                        thisreference.props.eventEmitter.emitEvent("UpdateChatMembers");
                    }
                });
            }
        }, {
            key: "handleOnClick",
            value: function handleOnClick(event) { }
        }, {
            key: "handleOnChange",
            value: function handleOnChange(event) { }
        }, {
            key: "render",
            value: function render() {
                chatUserNodes = "";

                if (this.state.InvitedUsers.length > 0) {
                    var tempthis = this;
                    var chatUserNodes = this.state.InvitedUsers.map(function (chatUser) {
                        return React.createElement("div", {
                            key: chatUser.key,
                            className: "col-12 userListItem"
                        }, React.createElement("span", null, chatUser.name), React.createElement("span", {
                            onClick: tempthis.handleOnRemoveClick,
                            "data-value": chatUser.name,
                            className: "button"
                        }, "remove"));
                    });
                    return React.createElement("div", null, React.createElement("div", {
                        className: "row",
                        key: this.props.key
                    }, chatUserNodes));
                }

                return React.createElement("p", null, " No Users Loaded");
            }
        }]);

        return InvitedUserList;
    }(React.Component);

var NameField =
    /*#__PURE__*/
    function (_React$Component4) {
        _inherits(NameField, _React$Component4);

        function NameField(props) {
            var _this4;

            _classCallCheck(this, NameField);

            _this4 = _possibleConstructorReturn(this, _getPrototypeOf(NameField).call(this, props));
            _this4.state = {
                value: props.value,
                id: props.owner
            };
            _this4.handleSubmit = _this4.handleSubmit.bind(_assertThisInitialized(_this4));
            _this4.handleFocus = _this4.handleFocus.bind(_assertThisInitialized(_this4));
            _this4.hangleOnChange = _this4.hangleOnChange.bind(_assertThisInitialized(_this4));
            return _this4;
        }

        _createClass(NameField, [{
            key: "componentDidUpdate",
            value: function componentDidUpdate(prevstate) {
                if (prevstate.value === "" && this.props.value !== "") {
                    this.setState({
                        value: this.props.value
                    });
                }

                if (prevstate.id === "undefined") {
                    this.setState({
                        id: this.props.owner
                    });
                }
            }
        }, {
            key: "handleSubmit",
            value: function handleSubmit(event) { }
        }, {
            key: "hangleOnChange",
            value: function hangleOnChange(event) {
                // update the state
                var inputName = event.target.value;
                this.setState({
                    value: inputName
                });
            }
        }, {
            key: "handleFocus",
            value: function handleFocus(event) {
                if (this.state.id !== undefined) {
                    var Url = "/Events/UpdateEventTitle/";
                    var data = new FormData();
                    $.ajax({
                        url: Url,
                        type: "POST",
                        data: {
                            Title: this.state.value,
                            ID: this.state.id
                        },
                        success: function success(result) { }
                    });
                }
            }
        }, {
            key: "render",
            value: function render() {
                return React.createElement("input", {
                    onBlur: this.handleFocus,
                    onChange: this.hangleOnChange,
                    className: "NameField",
                    type: "text",
                    value: this.state.value
                });
            }
        }]);

        return NameField;
    }(React.Component);

var Template =
    /*#__PURE__*/
    function (_React$Component5) {
        _inherits(Template, _React$Component5);

        function Template(props) {
            var _this5;

            _classCallCheck(this, Template);

            _this5 = _possibleConstructorReturn(this, _getPrototypeOf(Template).call(this, props));
            _this5.state = {
                day: props.day,
                start: props.start,
                stop: props.stop,
                name: props.name
            };
            _this5.handleOnDaySelect = _this5.handleOnDaySelect.bind(_assertThisInitialized(_this5));
            _this5.handleOnEndChange = _this5.handleOnEndChange.bind(_assertThisInitialized(_this5));
            _this5.handleOnStartChange = _this5.handleOnStartChange.bind(_assertThisInitialized(_this5));
            _this5.handleDelete = _this5.handleDelete.bind(_assertThisInitialized(_this5));
            return _this5;
        }

        _createClass(Template, [{
            key: "handleOnDaySelect",
            value: function handleOnDaySelect(event) {
                var tempthis = this;
                event.persist();
                var tempevent = event;
                $.ajax({
                    url: "/Events/SetDayOfTemplate/" + this.state.name,
                    type: "GET",
                    data: {
                        Day: event.target.value
                    },
                    success: function success(result) {
                        if (result === true) {
                            var day = parseInt(tempevent.target.value);
                            tempthis.setState({
                                day: day
                            });
                        }
                    }
                });
            }
        }, {
            key: "handleDelete",
            value: function handleDelete(event) {
                var tempthis = this;
                event.persist();
                $.ajax({
                    url: "/Events/RemoveTemplate/",
                    type: "GET",
                    data: {
                        ID: tempthis.state.name,
                        CalendarID: tempthis.props.calendar
                    },
                    success: function success(result) {
                        tempthis.props.eventEmitter.emitEvent("UpdateTemplates");
                    }
                });
            }
        }, {
            key: "handleOnStartChange",
            value: function handleOnStartChange(event) {
                var tempthis = this;
                event.persist();
                var value = event.target.value;
                $.ajax({
                    url: "/Events/SetStartOfTemplate/" + this.state.name,
                    type: "GET",
                    data: {
                        Start: event.target.value
                    },
                    success: function success(result) {
                        if (result === true) {
                            tempthis.setState({
                                start: value
                            });
                        }
                    }
                });
            }
        }, {
            key: "handleOnEndChange",
            value: function handleOnEndChange(event) {
                var tempthis = this;
                event.persist();
                var value = event.target.value;
                $.ajax({
                    url: "/Events/SetStopOfTemplate/" + this.state.name,
                    type: "GET",
                    data: {
                        Stop: event.target.value
                    },
                    success: function success(result) {
                        if (result === true) {
                            tempthis.setState({
                                stop: value
                            });
                        }
                    }
                });
            }
        }, {
            key: "render",
            value: function render() {
                var tempthis = this;
                var Days = [{
                    Day: "Montag",
                    Value: 1
                }, {
                    Day: "Dienstag",
                    Value: 2
                }, {
                    Day: "Mittwoch",
                    Value: 3
                }, {
                    Day: "Donnerstag",
                    Value: 4
                }, {
                    Day: "Freitag",
                    Value: 5
                }, {
                    Day: "Samstag",
                    Value: 6
                }, {
                    Day: "Sonntag",
                    Value: 7
                }];
                var templateNodes = Days.map(function (Day) {
                    return React.createElement("div", {
                        className: "day",
                        key: Day.Day + Day.Value
                    }, React.createElement("input", {
                        type: "radio",
                        onChange: tempthis.handleOnDaySelect,
                        checked: tempthis.state.day === Day.Value,
                        name: tempthis.state.name,
                        value: Day.Value
                    }), " ", React.createElement("span", null, Day.Day));
                });
                return React.createElement("div", {
                    className: "template"
                }, React.createElement("div", {
                    className: "days"
                }, templateNodes), React.createElement("div", {
                    className: "times"
                }, React.createElement("div", {
                    className: "time"
                }, React.createElement("span", null, "Start"), React.createElement("input", {
                    type: "time",
                    onChange: this.handleOnStartChange,
                    name: "Start",
                    value: this.state.start
                })), React.createElement("div", {
                    className: "time"
                }, React.createElement("span", null, "Ende"), React.createElement("input", {
                    type: "time",
                    onChange: this.handleOnEndChange,
                    name: "Stop",
                    value: this.state.stop
                }))), React.createElement("span", {
                    onClick: this.handleDelete,
                    className: "delete"
                }, React.createElement("i", {
                    className: "fas fa-trash-alt"
                })));
            }
        }]);

        return Template;
    }(React.Component);

var TemplateList =
    /*#__PURE__*/
    function (_React$Component6) {
        _inherits(TemplateList, _React$Component6);

        function TemplateList(props) {
            var _this6;

            _classCallCheck(this, TemplateList);

            _this6 = _possibleConstructorReturn(this, _getPrototypeOf(TemplateList).call(this, props));
            _this6.state = {
                Templates: []
            };
            _this6.handleAddTemplateClick = _this6.handleAddTemplateClick.bind(_assertThisInitialized(_this6));
            _this6.handleAddTemplateClick = _this6.handleAddTemplateClick.bind(_assertThisInitialized(_this6));
            _this6.handleUpdateTemplates = _this6.handleUpdateTemplates.bind(_assertThisInitialized(_this6));
            return _this6;
        }

        _createClass(TemplateList, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                $.ajax({
                    url: "/Events/Templates/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        thisreference.setState({
                            Templates: result
                        });
                    }
                });
                this.props.eventEmitter.addListener("UpdateTemplates", thisreference.handleUpdateTemplates);
            }
        }, {
            key: "handleUpdateTemplates",
            value: function handleUpdateTemplates(event) {
                var tempthis = this;
                $.ajax({
                    url: "/Events/Templates/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        tempthis.setState({
                            Templates: result
                        });
                    }
                });
            }
        }, {
            key: "handleAddTemplateClick",
            value: function handleAddTemplateClick(event) {
                var tempthis = this;
                $.ajax({
                    url: "/Events/AddTemplate/" + this.props.ID,
                    type: "GET",
                    data: {},
                    success: function success(result) {
                        var temptemplates = tempthis.state.Templates;
                        temptemplates.push(result);
                        tempthis.setState({
                            Templates: temptemplates
                        });
                    }
                });
            }
        }, {
            key: "render",
            value: function render() {
                if (this.state.Templates.length > 0) {
                    var tempthis = this;
                    var templateNodes = this.state.Templates.map(function (template) {
                        return React.createElement(Template, {
                            calendar: tempthis.props.ID,
                            eventEmitter: tempthis.props.eventEmitter,
                            key: template.key,
                            name: template.key,
                            day: template.Day,
                            start: template.Start,
                            stop: template.Stop
                        });
                    });
                    return React.createElement("div", null, React.createElement("div", {
                        className: "row",
                        key: this.props.key
                    }, React.createElement("div", {
                        className: "col-12"
                    }, React.createElement("span", {
                        onClick: this.handleAddTemplateClick,
                        className: "button"
                    }, "Add Template")), React.createElement("div", {
                        className: "col-12"
                    }, templateNodes)));
                }

                return React.createElement("div", {
                    className: "row"
                }, React.createElement("div", {
                    className: "col-12"
                }, React.createElement("span", {
                    onClick: this.handleAddTemplateClick,
                    className: "button"
                }, "Add Template")));
            }
        }]);

        return TemplateList;
    }(React.Component);