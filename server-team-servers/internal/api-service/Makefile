.PHONY: proto proto2

proto:
	docker run --platform linux/amd64 --rm -v ${PWD}:/srv -w /srv rvolosatovs/protoc:4 --proto_path pb pb/*.proto --go_out=pb --go-grpc_out=pb 