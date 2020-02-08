"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return !!right[Symbol.hasInstance](left); } else { return left instanceof right; } }

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!_instanceof(instance, Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

var Calendar =
    /*#__PURE__*/
    function (_React$Component) {
        _inherits(Calendar, _React$Component);

        function Calendar(props) {
            var _this;

            _classCallCheck(this, Calendar);

            _this = _possibleConstructorReturn(this, _getPrototypeOf(Calendar).call(this, props));
            _this.state = {
                data: [],
                eventEmitter: new EventEmitter(),
                mode: "default"
            };
            _this.changeMode = _this.changeMode.bind(_assertThisInitialized(_this));
            return _this;
        }

        _createClass(Calendar, [{
            key: "componentWillMount",
            value: function componentWillMount() {
                var thisreference = this;
                var xhr = new XMLHttpRequest();
                xhr.open('GET', "/Events/GetEventDates/" + this.props.ID, true);

                xhr.onload = function () {
                    thisreference.setState({
                        data: JSON.parse(xhr.responseText)
                    });
                };

                xhr.send();
            }
        }, {
            key: "changeMode",
            value: function changeMode(event) {
                this.setState({
                    mode: event.target.value
                });
            }
        }, {
            key: "render",
            value: function render() {
                if (this.state.data.Header !== undefined && this.state.data.Header.length > 0) {
                    var tempthis = this;
                    var headerNodes = this.state.data.Header.map(function (Header) {
                        return React.createElement(EventDate, {
                            mode: tempthis.state.mode,
                            key: Header.Date + Header.Time,
                            Data: Header
                        });
                    });
                    return React.createElement("div", null, React.createElement("select", {
                        className: "ml-3 mb-5",
                        onChange: this.changeMode
                    }, React.createElement("option", {
                        value: "default"
                    }, "Standard"), React.createElement("option", {
                        value: "fallback"
                    }, "Mobile Fallback")), React.createElement("div", {
                        className: "EventDateContainer"
                    }, headerNodes));
                } else {
                    return React.createElement("span", null, "Loading");
                }
            }
        }]);

        return Calendar;
    }(React.Component);

var EventDate =
    /*#__PURE__*/
    function (_React$Component2) {
        _inherits(EventDate, _React$Component2);

        function EventDate(props) {
            var _this2;

            _classCallCheck(this, EventDate);

            _this2 = _possibleConstructorReturn(this, _getPrototypeOf(EventDate).call(this, props));
            _this2.state = {
                Data: _this2.props.Data
            };
            return _this2;
        }

        _createClass(EventDate, [{
            key: "render",
            value: function render() {
                var tmpthis = this;
                var key = 0;
                var requestnodes = this.state.Data.Requests.map(function (request) {
                    key++;
                    return React.createElement("div", {
                        className: "row usernode mr-0 ml-0"
                    }, React.createElement("span", {
                        className: "col-6 pt-0 pb-0 pl-0 pr-0"
                    }, request.UserName), React.createElement(StateSelect, {
                        mode: tmpthis.props.mode,
                        key: key,
                        canEdit: request.canEdit,
                        requestID: request.AppointmentRequestID,
                        possibleStates: request.States,
                        state: request.State,
                        comment: request.Comment
                    }));
                });

                if (requestnodes.length > 0) {
                    return React.createElement("div", {
                        key: this.key,
                        className: "EventDate"
                    }, React.createElement("div", {
                        className: "row ml-0 mr-0"
                    }, React.createElement("span", {
                        className: "col-12 text-center bg_dark"
                    }, React.createElement("span", {
                        className: "date"
                    }, this.state.Data.Date), React.createElement("br", null), React.createElement("span", {
                        className: "time"
                    }, this.state.Data.Time))), requestnodes);
                } else {
                    return React.createElement("div", {
                        key: this.key,
                        className: "VoteUser"
                    }, React.createElement("span", {
                        className: "VoteUser_Name"
                    }, this.props.Name));
                }
            }
        }]);

        return EventDate;
    }(React.Component);

var StateSelect =
    /*#__PURE__*/
    function (_React$Component3) {
        _inherits(StateSelect, _React$Component3);

        function StateSelect(props) {
            var _this3;

            _classCallCheck(this, StateSelect);

            _this3 = _possibleConstructorReturn(this, _getPrototypeOf(StateSelect).call(this, props));
            _this3.state = {
                possibleStates: props.possibleStates,
                State: props.state,
                RetryCount: 0,
                comment: props.comment
            }; //this.handleOnChange = this.handleOnChange.bind(this);

            _this3.handleClick = _this3.handleClick.bind(_assertThisInitialized(_this3));
            return _this3;
        }

        _createClass(StateSelect, [{
            key: "handleClick",
            value: function handleClick(event) {
                event.target.classList.add("processing");
                var thisreference = this;
                var tmpevent = event;
                var element = event.target;
                var value = tmpevent.target.getAttribute("data-value");
                var comment = "";

                if (value === "3") {
                    comment = prompt("Kommentar eingeben", "");
                }

                thisreference.SyncStateToServer(thisreference.props.requestID, value, element, comment);
            }
        }, {
            key: "SyncStateToServer",
            value: function SyncStateToServer(ID, State, element, comment) {
                var thisreference = this;
                $.ajax({
                    url: "/Events/UpdateRequestState/",
                    type: "GET",
                    data: {
                        requestID: ID,
                        state: State,
                        comment: comment
                    },
                    success: function success(result) {
                        if (result > 0) {
                            thisreference.setState({
                                State: parseInt(element.getAttribute("data-value")),
                                comment: comment
                            });
                        } else {
                            if (thisreference.state.RetryCount === 3) {
                                confirm("Einer deiner Abstimmungen will grade wohl nicht. Später nochmal probieren");
                                thisreference.setState({
                                    RetryCount: 0
                                });
                            } else {
                                var newcount = thisreference.state.RetryCount + 1;
                                thisreference.setState({
                                    RetryCount: newcount
                                });
                                thisreference.SyncStateToServer(ID, State, element);
                            }
                        }

                        element.classList.remove("processing");
                    }
                });
            }
        }, {
            key: "render",
            value: function render() {
                var tmpthis = this;

                if (this.props.canEdit) {
                    if (this.state.possibleStates.length > 0) {
                        var states = this.state.possibleStates.map(function (state) {
                            if (state === "NotYetVoted") {
                                if (tmpthis.state.State === 0) {
                                    return React.createElement("span", {
                                        className: "voteoption active",
                                        "data-value": "0",
                                        key: tmpthis.props.key
                                    }, React.createElement("i", {
                                        className: "fas fa-minus"
                                    }));
                                } else {
                                    if (tmpthis.props.mode === "default") {
                                        return React.createElement("span", {
                                            className: "voteoption",
                                            "data-value": "0",
                                            onClick: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-minus"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    } else {
                                        return React.createElement("a", {
                                            href: "/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=0",
                                            target: "_blank",
                                            className: "voteoption",
                                            "data-value": "0",
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-minus"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    }
                                }
                            }

                            if (state === "Available") {
                                if (tmpthis.state.State === 1) {
                                    return React.createElement("span", {
                                        className: "voteoption greenbg active",
                                        "data-value": "1",
                                        key: tmpthis.props.key
                                    }, React.createElement("i", {
                                        className: "fas fa-check"
                                    }), React.createElement("span", {
                                        className: "lds-dual-ring"
                                    }));
                                } else {
                                    if (tmpthis.props.mode === "default") {
                                        return React.createElement("span", {
                                            className: "voteoption greenbg",
                                            "data-value": "1",
                                            onClick: tmpthis.handleClick,
                                            onTouchEnd: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-check"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    } else {
                                        return React.createElement("a", {
                                            href: "/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=1",
                                            target: "_blank",
                                            className: "voteoption greenbg",
                                            "data-value": "1",
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-check"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    }
                                }
                            }

                            if (state === "NotAvailable") {
                                if (tmpthis.state.State === 2) {
                                    return React.createElement("span", {
                                        className: "voteoption redbg active",
                                        "data-value": "2",
                                        key: tmpthis.props.key
                                    }, React.createElement("i", {
                                        className: "fas fa-times"
                                    }), React.createElement("span", {
                                        className: "lds-dual-ring"
                                    }));
                                } else {
                                    if (tmpthis.props.mode === "default") {
                                        return React.createElement("span", {
                                            className: "voteoption redbg",
                                            "data-value": "2",
                                            onClick: tmpthis.handleClick,
                                            onTouchEnd: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-times"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    } else {
                                        return React.createElement("a", {
                                            href: "/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=2",
                                            target: "_blank",
                                            className: "voteoption redbg",
                                            "data-value": "2",
                                            onClick: tmpthis.handleClick,
                                            onTouchEnd: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-times"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    }
                                }
                            }

                            if (state === "IfNeedBe") {
                                console.log(tmpthis.state);

                                if (tmpthis.state.State === 3) {
                                    return React.createElement("span", {
                                        className: "voteoption yellowbg active",
                                        "data-value": "3",
                                        key: tmpthis.props.key,
                                        onClick: tmpthis.handleClick,
                                        onTouchEnd: tmpthis.handleClick
                                    }, React.createElement("i", {
                                        className: "fas fa-question"
                                    }), React.createElement("span", {
                                        className: "lds-dual-ring"
                                    }), tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null && React.createElement("i", {
                                        className: "fas fa-info"
                                    }), tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null && React.createElement("span", {
                                        className: "commentBox"
                                    }, tmpthis.state.comment));
                                } else {
                                    if (tmpthis.props.mode === "default") {
                                        return React.createElement("span", {
                                            className: "voteoption yellowbg",
                                            "data-value": "3",
                                            onClick: tmpthis.handleClick,
                                            onTouchEnd: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-question"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    } else {
                                        return React.createElement("a", {
                                            href: "/Events/UpdateRequestState/?fallback=true&requestID=" + tmpthis.props.requestID + "&state=3",
                                            target: "_blank",
                                            className: "voteoption yellowbg",
                                            "data-value": "3",
                                            onClick: tmpthis.handleClick,
                                            onTouchEnd: tmpthis.handleClick,
                                            key: tmpthis.props.key
                                        }, React.createElement("i", {
                                            className: "fas fa-question"
                                        }), React.createElement("span", {
                                            className: "lds-dual-ring"
                                        }));
                                    }
                                }
                            }
                        });
                        return React.createElement("span", {
                            "data-state": this.state.State,
                            className: "requestNode col-6 pt-0 pb-0 pr-0 pl-0"
                        }, React.createElement("div", {
                            className: "d-flex"
                        }, states));
                    }
                } else {
                    switch (this.state.State) {
                        case 0:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-0 pb-0 pr-0 pl-0",
                                "data-state": this.state.State
                            }, React.createElement("div", {
                                className: "d-flex"
                            }, React.createElement("span", {
                                className: "voteoption voteoptionforeign",
                                key: tmpthis.props.key
                            }, React.createElement("i", {
                                className: "fas fa-minus"
                            }))));

                        case 1:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-0 pb-0 pr-0 pl-0",
                                "data-state": this.state.State
                            }, React.createElement("div", {
                                className: "d-flex"
                            }, React.createElement("span", {
                                className: "voteoption greenbg voteoptionforeign",
                                key: tmpthis.props.key
                            }, React.createElement("i", {
                                className: "fas fa-check"
                            }))));

                        case 2:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-0 pb-0 pr-0 pl-0",
                                "data-state": this.state.State
                            }, React.createElement("div", {
                                className: "d-flex"
                            }, React.createElement("span", {
                                className: "voteoption redbg voteoptionforeign",
                                key: tmpthis.props.key
                            }, React.createElement("i", {
                                className: "fas fa-times"
                            }))));

                        case 3:
                            return React.createElement("span", {
                                className: "requestNode col-6 pt-0 pb-0 pr-0 pl-0",
                                "data-state": this.state.State
                            }, React.createElement("div", {
                                className: "d-flex"
                            }, React.createElement("span", {
                                className: "voteoption yellowbg voteoptionforeign",
                                key: tmpthis.props.key
                            }, React.createElement("i", {
                                className: "fas fa-question"
                            }), tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null && React.createElement("i", {
                                className: "fas fa-info"
                            }), tmpthis.state.comment !== "" && tmpthis.state.comment !== undefined && tmpthis.state.comment !== null && React.createElement("span", {
                                className: "commentBox"
                            }, tmpthis.state.comment))));
                    }
                }

                return React.createElement("p", null, " No Users Loaded");
            }
        }]);

        return StateSelect;
    }(React.Component);