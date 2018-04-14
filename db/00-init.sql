CREATE EXTENSION pgcrypto;
SELECT gen_random_uuid();

CREATE TABLE TrackingNumbers (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	currentState integer NOT NULL DEFAULT 0,
	totalState integer NOT NULL DEFAULT 0,
	chatId bigint NOT NULL,
	trackingNumber varchar(30) NOT NULL DEFAULT ''
)