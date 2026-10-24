build-main:
	bash ./scripts/buildmains.sh
	
build-game:
	bash ./scripts/buildgames.sh

test-game-dfdc:
	# Copy config file
	cp ./internal/game/dfdc/config/gameserver.dat.example ./internal/game/dfdc/config/gameserver.dat
	cp ./internal/game/dfdc/config/serverinfo.dat.example ./internal/game/dfdc/config/serverinfo.dat
	cp ./internal/game/dfdc/config/slotconfig.dat.example ./internal/game/dfdc/config/slotconfig.dat

	mkdir -p .cover/html
	go test ./internal/game/dfdc -coverprofile=.cover/internal.game.dfdc.cover

test-game-tgw:
	# Copy config file
	cp ./internal/game/candyhouse/config/gameserver.dat.example ./internal/game/candyhouse/config/gameserver.dat
	cp ./internal/game/candyhouse/config/serverinfo.dat.example ./internal/game/candyhouse/config/serverinfo.dat
	cp ./internal/game/candyhouse/config/slotconfig.dat.example ./internal/game/candyhouse/config/slotconfig.dat

	mkdir -p .cover/html
	go test ./internal/game/candyhouse -coverprofile=.cover/internal.game.candyhouse.cover

test-game-ald:
	# Copy config file
	cp ./internal/game/aladdin/config/gameserver.dat.example ./internal/game/aladdin/config/gameserver.dat
	cp ./internal/game/aladdin/config/serverinfo.dat.example ./internal/game/aladdin/config/serverinfo.dat
	cp ./internal/game/aladdin/config/slotconfig.dat.example ./internal/game/aladdin/config/slotconfig.dat

	mkdir -p .cover/html
	go test ./internal/game/aladdin -coverprofile=.cover/internal.game.aladdin.cover

test-game-wlzb:
	# Copy config file
	cp ./internal/game/wlzb/config/gameserver.dat.example ./internal/game/wlzb/config/gameserver.dat
	cp ./internal/game/wlzb/config/serverinfo.dat.example ./internal/game/wlzb/config/serverinfo.dat
	cp ./internal/game/wlzb/config/slotconfig.dat.example ./internal/game/wlzb/config/slotconfig.dat

	mkdir -p .cover/html
	go test ./internal/game/wlzb -coverprofile=.cover/internal.game.wlzb.cover