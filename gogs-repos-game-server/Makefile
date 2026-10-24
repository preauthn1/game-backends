test:

test-game-tgw:
	# Copy config file
	cp ./game/tgw/config/gameserver.dat.example ./game/tgw/config/gameserver.dat
	cp ./game/tgw/config/serverinfo.dat.example ./game/tgw/config/serverinfo.dat
	cp ./game/tgw/config/slotconfig.dat.example ./game/tgw/config/slotconfig.dat

	go test candyhouse -cover

test-game-ald:
	# Copy config file
	cp ./src/example.invalid/server/internal/game/aladdin/config/gameserver.dat.example ./src/example.invalid/server/internal/game/aladdin/config/gameserver.dat
	cp ./src/example.invalid/server/internal/game/aladdin/config/serverinfo.dat.example ./src/example.invalid/server/internal/game/aladdin/config/serverinfo.dat
	cp ./src/example.invalid/server/internal/game/aladdin/config/slotconfig.dat.example ./src/example.invalid/server/internal/game/aladdin/config/slotconfig.dat

	go test -cover example.invalid/server/internal/game/aladdin