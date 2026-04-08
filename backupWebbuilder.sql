--
-- PostgreSQL database dump
--

\restrict 0imDjUhYcnZh0fcolobqv6iJ8VBplFgtPiwnU7bbLG6GMBTTTHLJSB4n4l0rO7W

-- Dumped from database version 15.15
-- Dumped by pg_dump version 15.15

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: projects; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.projects (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "DateOpen" timestamp with time zone NOT NULL,
    "TextHtml" text NOT NULL
);


ALTER TABLE public.projects OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    "Id" uuid NOT NULL,
    "Login" text NOT NULL,
    "HashPassword" text NOT NULL,
    "Role" text NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260408120622_InitialCreate	10.0.0
\.


--
-- Data for Name: projects; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.projects ("Id", "Name", "DateOpen", "TextHtml") FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users ("Id", "Login", "HashPassword", "Role") FROM stdin;
438e452d-a8db-4c7a-b4b8-4ada7cda7d76	admin@mail.ru	$2a$11$cXzJITgtUiw/4cWi1y.XH.xHG01Bwyj53m3w2HOU4nWIrOk24AgXG	admin
c93c0103-14f1-43b0-9a0c-5862624bbd9b	user@mail.ru	$2a$11$zD.sI1v4tUcoNEHXYH/vduhMU8kiFMXJh5yURxIkE5S2emBiS156i	user
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: projects PK_projects; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.projects
    ADD CONSTRAINT "PK_projects" PRIMARY KEY ("Id");


--
-- Name: users PK_users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "PK_users" PRIMARY KEY ("Id");


--
-- PostgreSQL database dump complete
--

\unrestrict 0imDjUhYcnZh0fcolobqv6iJ8VBplFgtPiwnU7bbLG6GMBTTTHLJSB4n4l0rO7W

