-- MySQL Script generated by MySQL Workbench
-- 09/04/19 09:48:35
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
  `openeye_name` VARCHAR(45) NULL,
  `cas_name` VARCHAR(45) NULL,
  `name_markup` VARCHAR(45) NULL,
  `name` VARCHAR(45) NULL,
  `systematic_name` VARCHAR(45) NULL,
  `traditional_name` VARCHAR(45) NULL,
  `inchi` VARCHAR(45) NULL,
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
  `hbond_acceptor` VARCHAR(45) NULL,
  `hbond_donor` VARCHAR(45) NULL,
  `rotatable_bond` VARCHAR(45) NULL,
  `subkeys` VARCHAR(45) NULL,
  `xlogp3_aa` VARCHAR(45) NULL,
  `exact_mass` VARCHAR(45) NULL,
  `formula` VARCHAR(45) NULL,
  `molecular_weight` VARCHAR(45) NULL,
  `can_smiles` VARCHAR(45) NULL,
  `iso_smiles` VARCHAR(45) NULL,
  `tpsa` VARCHAR(45) NULL,
  `monoisotopic_weight` VARCHAR(45) NULL,
  `total_charge` VARCHAR(45) NULL,
  `heavy_atom_count` VARCHAR(45) NULL,
  `atom_def_stereo_count` VARCHAR(45) NULL,
  `atom_udef_stereo_count` VARCHAR(45) NULL,
  `bond_def_stereo_count` VARCHAR(45) NULL,
  `bond_udef_stereo_count` VARCHAR(45) NULL,
  `isotopic_atom_count` VARCHAR(45) NULL,
  `component_count` VARCHAR(45) NULL,
  `tauto_count` VARCHAR(45) NULL,
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
  `checksum` VARCHAR(45) NULL COMMENT 'md5',
  `model_base64` LONGTEXT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB;

SHOW WARNINGS;

-- -----------------------------------------------------
-- Table `pubchem`.`compound`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pubchem`.`compound` (
  `cid` INT NOT NULL COMMENT 'The pubchem cid',
  `canonicalized` INT NOT NULL DEFAULT 0,
  `common_name` VARCHAR(45) NULL,
  `kegg` VARCHAR(45) NULL,
  `hmdb` VARCHAR(45) NULL,
  `chebi` VARCHAR(45) NULL,
  `inchi_key` CHAR(32) NOT NULL,
  PRIMARY KEY (`cid`),
  UNIQUE INDEX `cid_UNIQUE` (`cid` ASC))
ENGINE = InnoDB
COMMENT = 'table for query database. 这个表比较精简，主要是为了方便进行根据id或者inchi_key以及其他的数据库编号来进行的快速查询操作';

SHOW WARNINGS;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
