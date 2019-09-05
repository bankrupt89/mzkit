-- MySQL Script generated by MySQL Workbench
-- 09/05/19 12:27:23
-- Model: New Model    Version: 1.0
-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema pubchem
-- -----------------------------------------------------
DROP SCHEMA IF EXISTS `pubchem` ;

-- -----------------------------------------------------
-- Schema pubchem
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `pubchem` DEFAULT CHARACTER SET utf8 ;
SHOW WARNINGS;
USE `pubchem` ;

-- -----------------------------------------------------
-- Table `pubchem`.`IUPAC`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`IUPAC` (
  `cid` INT NOT NULL,
  `openeye_name` VARCHAR(256) NULL,
  `cas_name` VARCHAR(256) NULL,
  `name_markup` LONGTEXT NULL,
  `name` VARCHAR(256) NULL,
  `systematic_name` LONGTEXT NULL,
  `traditional_name` VARCHAR(256) NULL,
  `inchi` LONGTEXT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`descriptor`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`descriptor` (
  `cid` INT NOT NULL,
  `complexity` FLOAT NULL,
  `hbond_acceptor` INT NULL,
  `hbond_donor` INT NULL,
  `rotatable_bond` INT NULL,
  `subkeys` VARCHAR(2048) NULL,
  `xlogp3_aa` FLOAT NULL,
  `exact_mass` FLOAT NULL,
  `formula` VARCHAR(128) NULL,
  `molecular_weight` FLOAT NULL,
  `can_smiles` VARCHAR(512) NULL,
  `iso_smiles` VARCHAR(512) NULL,
  `tpsa` FLOAT NULL,
  `monoisotopic_weight` FLOAT NULL,
  `total_charge` FLOAT NULL,
  `heavy_atom_count` INT NULL,
  `atom_def_stereo_count` INT NULL,
  `atom_udef_stereo_count` INT NULL,
  `bond_def_stereo_count` INT NULL,
  `bond_udef_stereo_count` INT NULL,
  `isotopic_atom_count` INT NULL,
  `component_count` INT NULL,
  `tauto_count` INT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`structure`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`structure` (
  `cid` INT NOT NULL,
  `coordinate_type` VARCHAR(1024) NULL,
  `bond_annotations` VARCHAR(1024) NULL,
  `checksum` VARCHAR(32) NULL COMMENT 'md5',
  `model_base64` LONGTEXT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`synonym`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`synonym` (
  `cid` INT NOT NULL,
  `synonym_name` VARCHAR(1024) NOT NULL,
  PRIMARY KEY (`cid`))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`cas_registry`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`cas_registry` (
  `cid` INT NOT NULL,
  `cas_number` VARCHAR(64) NOT NULL,
  PRIMARY KEY (`cid`))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`compound`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`compound` (
  `cid` INT NOT NULL COMMENT 'The pubchem cid',
  `canonicalized` INT NOT NULL DEFAULT 0,
  `common_name` VARCHAR(256) NULL,
  `kegg` VARCHAR(8) NULL,
  `hmdb` VARCHAR(16) NULL,
  `chebi` VARCHAR(16) NULL,
  `inchi_key` CHAR(32) NOT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB
COMMENT = 'table for query database. 这个表比较精简，主要是为了方便进行根据id或者inchi_key以及其他的数据库编号来进行的快速查询操作';

SHOW WARNINGS;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
